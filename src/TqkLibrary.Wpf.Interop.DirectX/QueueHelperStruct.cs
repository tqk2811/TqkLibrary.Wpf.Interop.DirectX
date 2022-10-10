using System;
using System.Runtime.InteropServices;

namespace TqkLibrary.Wpf.Interop.DirectX
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct QueueHelperStruct
    {
        public IntPtr pDXGISurface;
        public IntPtr pUnkDXGISurface;

        public IntPtr pTexture9;
        public IntPtr pUnkTexture9;

        public IntPtr pSurface9;

        public int Count;
    }
}
