#ifndef _H_SurfaceQueueInterop_H_
#define _H_SurfaceQueueInterop_H_

struct SurfaceQueueInterop
{
    UINT32 m_pixelWidth;
    UINT32 m_pixelHeight;

    HWND m_hwnd;

    IDirect3D9Ex* m_pD3D9;
    IDirect3DDevice9Ex* m_pD3D9Device;

    ID3D10Device1* m_D3D10Device;

    ISurfaceQueue* m_ABQueue;
    ISurfaceQueue* m_BAQueue;

    ISurfaceConsumer* m_ABConsumer;
    ISurfaceProducer* m_BAProducer;
    ISurfaceConsumer* m_BAConsumer;
    ISurfaceProducer* m_ABProducer;

    bool m_isD3DInitialized;
    bool m_areSurfacesInitialized;
    bool m_shouldSkipRender;
};

#endif // !_H_SurfaceQueueInterop_H_
