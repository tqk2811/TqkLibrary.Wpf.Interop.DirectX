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

        [MarshalAs(UnmanagedType.U4)]
        public uint m_numSurfaces;

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

        // These are 1-byte flags (0/1) rather than bool so the whole struct stays blittable;
        // a bool field would force the marshaler to copy the struct in/out on every P/Invoke
        // call (this struct is passed by ref several times per rendered frame).
        public byte m_isD3DInitialized;
        public byte m_areSurfacesInitialized;
        public byte m_shouldSkipRender;
    }
}
