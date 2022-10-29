#include "pch.h"
#include "Exports.h"

#define WIDTH 640
#define HEIGHT 480

REFIID                  surfaceIDDXGI = __uuidof(IDXGISurface);
REFIID                  surfaceID9 = __uuidof(IDirect3DTexture9);

IUnknown* ReleaseInterface(IUnknown* pointer) {
	if (NULL != pointer) {
		pointer->Release();
		pointer = NULL;
	}
	return pointer;
}

HRESULT InitD3D10(SurfaceQueueInterop& surfaceQueueInterop)
{
	HRESULT hr;
	UINT		DeviceFlags = D3D10_CREATE_DEVICE_BGRA_SUPPORT;
	//DWORD		dwShaderFlags = D3D10_SHADER_ENABLE_STRICTNESS;
#ifdef _DEBUG
	// To debug DirectX, uncomment the following lines:

	//DeviceFlags |= D3D10_CREATE_DEVICE_DEBUG;
	//dwShaderFlags	|= D3D10_SHADER_DEBUG;
#endif

	ID3D10Device1** ppD3D10Device = &surfaceQueueInterop.m_D3D10Device;
	if (FAILED(hr = D3D10CreateDevice1(NULL, D3D10_DRIVER_TYPE_HARDWARE, NULL,
		DeviceFlags, D3D10_FEATURE_LEVEL_10_0, D3D10_1_SDK_VERSION, ppD3D10Device)))
	{
		return hr;
	}

	D3D10_VIEWPORT vp;
	vp.Width = WIDTH;
	vp.Height = HEIGHT;
	vp.MinDepth = 0.0f;
	vp.MaxDepth = 1.0f;
	vp.TopLeftX = 0;
	vp.TopLeftY = 0;
	surfaceQueueInterop.m_D3D10Device->RSSetViewports(1, &vp);

	return S_OK;
}

HRESULT InitD3D9(SurfaceQueueInterop& surfaceQueueInterop)
{
	HRESULT hr;

	IDirect3D9Ex** ppD3D9 = &surfaceQueueInterop.m_pD3D9;
	Direct3DCreate9Ex(D3D_SDK_VERSION, ppD3D9);

	if (!surfaceQueueInterop.m_pD3D9)
	{
		return E_FAIL;
	}

	D3DPRESENT_PARAMETERS d3dpp;
	ZeroMemory(&d3dpp, sizeof(d3dpp));
	d3dpp.Windowed = TRUE;
	d3dpp.SwapEffect = D3DSWAPEFFECT_DISCARD;
	d3dpp.hDeviceWindow = NULL;
	d3dpp.PresentationInterval = D3DPRESENT_INTERVAL_IMMEDIATE;

	{
		IDirect3DDevice9Ex** ppD3D9Device = &surfaceQueueInterop.m_pD3D9Device;

		hr = surfaceQueueInterop.m_pD3D9->CreateDeviceEx(
			D3DADAPTER_DEFAULT,
			D3DDEVTYPE_HAL,
			surfaceQueueInterop.m_hwnd,
			D3DCREATE_HARDWARE_VERTEXPROCESSING | D3DCREATE_MULTITHREADED | D3DCREATE_FPU_PRESERVE,
			&d3dpp,
			NULL,
			ppD3D9Device);
	}

	return hr;
}

HRESULT InitSurfaces(SurfaceQueueInterop& surfaceQueueInterop)
{
	HRESULT hr = S_OK;

	SURFACE_QUEUE_DESC  desc;
	ZeroMemory(&desc, sizeof(desc));
	desc.Width = surfaceQueueInterop.m_pixelWidth;
	desc.Height = surfaceQueueInterop.m_pixelHeight;
	desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
	desc.NumSurfaces = 1;
	desc.MetaDataSize = sizeof(int);
	desc.Flags = SURFACE_QUEUE_FLAG_SINGLE_THREADED;

	SURFACE_QUEUE_CLONE_DESC CloneDesc = { 0 };
	CloneDesc.MetaDataSize = 0;
	CloneDesc.Flags = SURFACE_QUEUE_FLAG_SINGLE_THREADED;

	if (!surfaceQueueInterop.m_isD3DInitialized || (desc.Width <= 0) || (desc.Height <= 0))
	{
		hr = S_FALSE;
		goto Cleanup;
	}

	if (!surfaceQueueInterop.m_areSurfacesInitialized)
	{
		{
			ISurfaceQueue** ppABQueue = &surfaceQueueInterop.m_ABQueue;
			IFC(CreateSurfaceQueue(&desc, surfaceQueueInterop.m_pD3D9Device, ppABQueue));
		}

		// Clone the queue
		{
			ISurfaceQueue** ppBAQueue = &surfaceQueueInterop.m_BAQueue;
			IFC(surfaceQueueInterop.m_ABQueue->Clone(&CloneDesc, ppBAQueue));
		}

		// Setup queue management
		{
			ISurfaceProducer** ppm_BAProducer = &surfaceQueueInterop.m_BAProducer;
			IFC(surfaceQueueInterop.m_BAQueue->OpenProducer(surfaceQueueInterop.m_D3D10Device, ppm_BAProducer));
		}

		{
			ISurfaceConsumer** ppm_ABConsumer = &surfaceQueueInterop.m_ABConsumer;
			IFC(surfaceQueueInterop.m_ABQueue->OpenConsumer(surfaceQueueInterop.m_D3D10Device, ppm_ABConsumer));
		}

		{
			ISurfaceProducer** ppm_ABProducer = &surfaceQueueInterop.m_ABProducer;
			IFC(surfaceQueueInterop.m_ABQueue->OpenProducer(surfaceQueueInterop.m_pD3D9Device, ppm_ABProducer));
		}

		{
			ISurfaceConsumer** ppm_BAConsumer = &surfaceQueueInterop.m_BAConsumer;
			IFC(surfaceQueueInterop.m_BAQueue->OpenConsumer(surfaceQueueInterop.m_pD3D9Device, ppm_BAConsumer));
		}

		surfaceQueueInterop.m_areSurfacesInitialized = true;
	}

Cleanup:

	return hr;
}

HRESULT _IDirect3DDevice9Ex_CheckDeviceState(IDirect3DDevice9Ex* pD3D9Device)
{
	return pD3D9Device->CheckDeviceState(NULL);
}

HRESULT QueueHelper_GetDXGISurface(SurfaceQueueInterop& surfaceQueueInterop, QueueHelperStruct& queueHelperStruct)
{
	HRESULT hr = S_OK;
	UINT size = sizeof(int);
	DXGI_SURFACE_DESC desc;
	// Flush the AB queue
	surfaceQueueInterop.m_ABProducer->Flush(0 /* wait */, NULL);
	// Dequeue from AB queue
	hr = surfaceQueueInterop.m_ABConsumer->Dequeue(surfaceIDDXGI, &queueHelperStruct.pUnkDXGISurface, &queueHelperStruct.count, &size, INFINITE);
	if (FAILED(hr)) return hr;

	hr = queueHelperStruct.pUnkDXGISurface->QueryInterface(surfaceIDDXGI, (void**)&queueHelperStruct.pDXGISurface);
	if (FAILED(hr)) return hr;

	hr = queueHelperStruct.pDXGISurface->GetDesc(&desc);

	return hr;
}

HRESULT QueueHelper_GetSurface9(SurfaceQueueInterop& surfaceQueueInterop, QueueHelperStruct& queueHelperStruct)
{
	HRESULT hr = S_OK;

	// Produce the surface
	hr = surfaceQueueInterop.m_BAProducer->Enqueue(queueHelperStruct.pDXGISurface, NULL, NULL, SURFACE_QUEUE_FLAG_DO_NOT_WAIT);
	if (FAILED(hr)) return hr;

	// Flush the BA queue
	hr = surfaceQueueInterop.m_BAProducer->Flush(0 /* wait, *not* SURFACE_QUEUE_FLAG_DO_NOT_WAIT*/, NULL);
	if (FAILED(hr)) return hr;

	// Dequeue from BA queue
	hr = surfaceQueueInterop.m_BAConsumer->Dequeue(surfaceID9, &queueHelperStruct.pUnkTexture9, NULL, NULL, INFINITE);
	if (FAILED(hr)) return hr;

	hr = queueHelperStruct.pUnkTexture9->QueryInterface(surfaceID9, (void**)&queueHelperStruct.pTexture9);
	if (FAILED(hr)) return hr;

	// Get the top level surface from the texture
	hr = queueHelperStruct.pTexture9->GetSurfaceLevel(0, &queueHelperStruct.pSurface9);

	return hr;
}
HRESULT QueueHelper_ABProducerEnqueueTexture9(SurfaceQueueInterop& surfaceQueueInterop, QueueHelperStruct& queueHelperStruct)
{
	HRESULT hr = S_OK;
	// Produce Surface
	hr = surfaceQueueInterop.m_ABProducer->Enqueue(queueHelperStruct.pTexture9, &queueHelperStruct.count, sizeof(int), SURFACE_QUEUE_FLAG_DO_NOT_WAIT);
	if (FAILED(hr)) return hr;

	// Flush the AB queue - use "do not wait" here, we'll block at the top of the *next* call if we need to
	hr = surfaceQueueInterop.m_ABProducer->Flush(SURFACE_QUEUE_FLAG_DO_NOT_WAIT, NULL);

	return hr;
}
void QueueHelper_Release(QueueHelperStruct& queueHelperStruct)
{
	_ReleaseInterface(queueHelperStruct.pSurface9);

	_ReleaseInterface(queueHelperStruct.pTexture9);
	_ReleaseInterface(queueHelperStruct.pUnkTexture9);

	_ReleaseInterface(queueHelperStruct.pDXGISurface);
	_ReleaseInterface(queueHelperStruct.pUnkDXGISurface);
}