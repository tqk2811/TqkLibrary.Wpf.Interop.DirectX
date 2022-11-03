using System;
using System.Runtime.InteropServices;

namespace TqkLibrary.Wpf.Interop.DirectX
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct QueueHelperStruct
    {
        public IntPtr pDXGISurface;//IDXGISurface
        public IntPtr pUnkDXGISurface;//IUnknown

        public IntPtr pTexture9;//IDirect3DTexture9
        public IntPtr pUnkTexture9;//IUnknown

        public IntPtr pSurface9;//IDirect3DSurface9

        [MarshalAs(UnmanagedType.U4)]
        public int Count;
    }
}
