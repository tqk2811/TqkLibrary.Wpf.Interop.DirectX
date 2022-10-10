using System;
using System.Runtime.InteropServices;

namespace TqkLibrary.Wpf.Interop.DirectX
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SurfaceQueueInterop
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint m_pixelWidth;

        [MarshalAs(UnmanagedType.U4)]
        public uint m_pixelHeight;

        public IntPtr m_hwnd;

        public IntPtr m_pD3D9;//IDirect3D9Ex
        public IntPtr m_pD3D9Device;//IDirect3DDevice9Ex

        public IntPtr m_D3D10Device;//ID3D10Device1

        public IntPtr m_ABQueue;//ISurfaceQueue
        public IntPtr m_BAQueue;//ISurfaceQueue

        public IntPtr m_ABConsumer;//ISurfaceConsumer
        public IntPtr m_BAProducer;//ISurfaceProducer
        public IntPtr m_BAConsumer;//ISurfaceConsumer
        public IntPtr m_ABProducer;//ISurfaceProducer

        [MarshalAs(UnmanagedType.U1)]
        public bool m_isD3DInitialized;
        [MarshalAs(UnmanagedType.U1)]
        public bool m_areSurfacesInitialized;
        [MarshalAs(UnmanagedType.U1)]
        public bool m_shouldSkipRender;
    }
}
