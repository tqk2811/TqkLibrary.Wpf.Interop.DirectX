#ifndef _H_QueueHelperStruct_H_
#define _H_QueueHelperStruct_H_

struct QueueHelperStruct
{
    IDXGISurface* pDXGISurface;
    IUnknown* pUnkDXGISurface;

    IDirect3DTexture9* pTexture9;
    IUnknown* pUnkTexture9;
    
    IDirect3DSurface9* pSurface9;

    int count;
};

#endif // !_H_SurfaceQueueInterop_H_
