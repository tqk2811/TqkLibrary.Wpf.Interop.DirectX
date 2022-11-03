#ifndef TQKLIBRARYWPFINTEROPDIRECTXNATIVE_Exports_H
#define TQKLIBRARYWPFINTEROPDIRECTXNATIVE_Exports_H

#ifdef TQKLIBRARYWPFINTEROPDIRECTXNATIVE_EXPORTS
#define TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT extern "C" __declspec( dllexport )
#else
#define TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT extern "C" __declspec( dllimport )
#endif


#define WIN32_LEAN_AND_MEAN// Exclude rarely-used stuff from Windows headers
#include <windows.h>
#include <comdef.h>

// C RunTime Header Files
#include <stdlib.h>
#include <stdio.h>

#if _DEBUG
#define D3D_DEBUG_INFO
#endif

#if DIRECTX_SDK
#include <d3dx9.h>
#else
#include "d3d9.h"
#endif

#include <D3D10_1.h>

#define IFC(x) { hr = (x); if (FAILED(hr)) { goto Cleanup; }}
#define _ReleaseInterface(x) { if (NULL != x) { x->Release(); x = NULL; }}

#include "SurfaceQueue.h"


#include "SurfaceQueueInterop.h"
#include "QueueHelperStruct.h"


TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT HRESULT InitD3D10(SurfaceQueueInterop& surfaceQueueInterop);

TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT HRESULT InitD3D9(SurfaceQueueInterop& surfaceQueueInterop);

TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT HRESULT InitSurfaces(SurfaceQueueInterop& surfaceQueueInterop);

TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT HRESULT _IDirect3DDevice9Ex_CheckDeviceState(IDirect3DDevice9Ex* pD3D9Device);



TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT HRESULT QueueHelper_GetDXGISurface(SurfaceQueueInterop& surfaceQueueInterop, QueueHelperStruct& queueHelperStruct);

TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT HRESULT QueueHelper_GetSurface9(SurfaceQueueInterop& surfaceQueueInterop, QueueHelperStruct& queueHelperStruct);

TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT HRESULT QueueHelper_ABProducerEnqueueTexture9(SurfaceQueueInterop& surfaceQueueInterop, QueueHelperStruct& queueHelperStruct);

TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT void QueueHelper_Release(QueueHelperStruct& queueHelperStruct);



TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT IUnknown* ReleaseInterface(IUnknown* pointer);

#endif // !Exports
