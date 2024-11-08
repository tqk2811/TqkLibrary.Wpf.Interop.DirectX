﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.Wpf.Interop.DirectX
{
    internal static class NativeWrapper
    {
        const string _dllName = "TqkLibrary.Wpf.Interop.DirectX.Native.dll";
#if DEBUG
        static NativeWrapper()
        {
            string path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                Environment.Is64BitProcess ? "x64" : "x86");

            bool r = SetDllDirectory(path);
            if (!r)
                throw new InvalidOperationException("Can't set Kernel32.SetDllDirectory");
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool SetDllDirectory(string PathName);
#endif


        [DllImport(_dllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ReleaseInterface(ref IntPtr pointer);


        [DllImport(_dllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int InitD3D10(ref SurfaceQueueInterop surfaceQueueInterop);

        [DllImport(_dllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int InitD3D9(ref SurfaceQueueInterop surfaceQueueInterop);

        [DllImport(_dllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int InitSurfaces(ref SurfaceQueueInterop surfaceQueueInterop);

        [DllImport(_dllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int _IDirect3DDevice9Ex_CheckDeviceState(IntPtr pointer);

        [DllImport(_dllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int QueueHelper_GetDXGISurface(ref SurfaceQueueInterop surfaceQueueInterop, ref QueueHelperStruct queueHelperStruct);

        [DllImport(_dllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int QueueHelper_GetSurface9(ref SurfaceQueueInterop surfaceQueueInterop, ref QueueHelperStruct queueHelperStruct);

        [DllImport(_dllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int QueueHelper_ABProducerEnqueueTexture9(ref SurfaceQueueInterop surfaceQueueInterop, ref QueueHelperStruct queueHelperStruct);

        [DllImport(_dllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void QueueHelper_Release(ref QueueHelperStruct queueHelperStruct);
    }
}
