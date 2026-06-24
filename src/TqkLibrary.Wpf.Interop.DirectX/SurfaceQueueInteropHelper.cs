using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace TqkLibrary.Wpf.Interop.DirectX
{
    internal class SurfaceQueueInteropHelper : IDisposable
    {
        SurfaceQueueInterop m_native = new SurfaceQueueInterop();
        OnRenderDelegate m_renderD2D;
        D3DImage m_d3dImage;
        bool _disposed;
        //DependencyPropertyChangedEventHandler m_frontBufferAvailableChanged;


        public OnRenderDelegate RenderD2D
        {
            set { m_renderD2D = value; }
        }
        public D3DImage D3DImage
        {
            get { return m_d3dImage; }
            set
            {
                if (value != m_d3dImage)
                {
                    if (m_d3dImage is not null)
                    {
                        m_d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                    }

                    m_d3dImage = value;

                    // TODO: Force a rerender...?
                }
            }
        }

        public uint PixelWidth
        {
            get { return m_native.m_pixelWidth; }
        }
        public uint PixelHeight
        {
            get { return m_native.m_pixelHeight; }
        }
        public IntPtr HWND
        {
            get { return m_native.m_hwnd; }
            set { m_native.m_hwnd = value; }
        }

        ~SurfaceQueueInteropHelper()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;

            // In a finalizer during process/AppDomain shutdown we run on the GC thread and
            // the native DLL may already be unloaded; the OS reclaims everything, so don't
            // risk calling into native code. Release only on a deterministic Dispose, or while
            // the process is still running normally.
            if (!disposing && (Environment.HasShutdownStarted || AppDomain.CurrentDomain.IsFinalizingForUnload()))
                return;

            CleanupD3D();
        }

        public void SetPixelSize(uint pixelWidth, uint pixelHeight)
        {
            if ((m_native.m_pixelWidth != pixelWidth) || (m_native.m_pixelHeight != pixelHeight))
            {
                m_native.m_pixelWidth = pixelWidth;
                m_native.m_pixelHeight = pixelHeight;
                CleanupSurfaces();
                QueueHelper(QueueRenderMode.RenderDXGI);
            }
        }
        public void RequestRenderD2D()
        {
            QueueHelper(QueueRenderMode.RenderDXGI);
        }
        public uint NumSurfaces
        {
            get { return m_native.m_numSurfaces; }
            set
            {
                uint count = value < 1 ? 1 : value;
                if (m_native.m_numSurfaces != count)
                {
                    m_native.m_numSurfaces = count;
                    // The buffer count changed; drop the surfaces so the next render recreates
                    // the queue with the new count.
                    CleanupSurfaces();
                }
            }
        }

        bool FAILED(int hr) => hr < 0;

        void RenderToDXGI(IntPtr pdxgiSurface, bool isNewSurface) => m_renderD2D?.Invoke(pdxgiSurface, isNewSurface);

        void CleanupD3D10()
        {
            NativeWrapper.ReleaseInterface(ref m_native.m_D3D10Device);
        }

        void CleanupD3D9()
        {
            NativeWrapper.ReleaseInterface(ref m_native.m_pD3D9Device);
            NativeWrapper.ReleaseInterface(ref m_native.m_pD3D9);
        }

        void CleanupSurfaces()
        {
            m_native.m_areSurfacesInitialized = 0;

            NativeWrapper.ReleaseInterface(ref m_native.m_BAProducer);
            NativeWrapper.ReleaseInterface(ref m_native.m_ABProducer);
            NativeWrapper.ReleaseInterface(ref m_native.m_BAConsumer);
            NativeWrapper.ReleaseInterface(ref m_native.m_ABConsumer);

            NativeWrapper.ReleaseInterface(ref m_native.m_ABQueue);
            NativeWrapper.ReleaseInterface(ref m_native.m_BAQueue);
        }

        void CleanupD3D()
        {
            if (m_native.m_areSurfacesInitialized != 0)
            {
                CleanupSurfaces();
            }

            m_native.m_isD3DInitialized = 0;

            CleanupD3D10();
            CleanupD3D9();
        }

        int /*HRESULT*/ InitD3D()
        {
            int hr = 0;
            if (m_native.m_isD3DInitialized == 0)
            {
                hr = NativeWrapper.InitD3D9(ref m_native);
                if (FAILED(hr))
                    goto Cleanup;

                hr = NativeWrapper.InitD3D10(ref m_native);
                if (FAILED(hr))
                    goto Cleanup;

                m_native.m_isD3DInitialized = 1;
            }

        Cleanup:
            // Clean up, but don't throw, as this can be a transient failure.
            // TODO: Consider if/how to differentiate between fatal failure and transient failure.
            if (FAILED(hr))
            {
                CleanupD3D();
            }
            return hr;
        }

        const int S_OK = 0;
        const int D3D_OK = S_OK;
        const int E_FAIL = unchecked((int)0x80004005);
        bool Initialize()
        {
            int hr = S_OK;

            if (m_native.m_isD3DInitialized != 0)
            {
                hr = NativeWrapper._IDirect3DDevice9Ex_CheckDeviceState(m_native.m_pD3D9Device);

                if (D3D_OK != hr)
                {
                    CleanupD3D();
                }
            }

            if (m_native.m_isD3DInitialized == 0)
            {
                hr = InitD3D();
                if (FAILED(hr))
                    goto Cleanup;
            }

            if (m_native.m_areSurfacesInitialized == 0)
            {
                // Can be S_FALSE if there's nothing to do.
                hr = NativeWrapper.InitSurfaces(ref m_native);
                if (FAILED(hr))
                    goto Cleanup;
            }

        Cleanup:
            // Clean up, but don't throw, as this can be a transient failure.
            // TODO: Consider if/how to differentiate between fatal failure and transient failure.
            if (FAILED(hr))
            {
                CleanupD3D();
            }
            return m_native.m_areSurfacesInitialized != 0;
        }

        // If fShouldRenderD3D10 is true, this method performs the callout to RenderD3D10.
        // In any case, this method always initializes m_d3dImage which incurrs no cost if this results in no change.

        void QueueHelper(QueueRenderMode renderMode)
        {
            bool isNewSurface = m_native.m_areSurfacesInitialized == 0;

            if (_disposed || m_native.m_shouldSkipRender != 0 || m_d3dImage is null || !Initialize())
                return;

            QueueHelperStruct queueHelperStruct = new QueueHelperStruct();
            m_d3dImage.Lock();
            try
            {
                int hr = NativeWrapper.QueueHelper_GetDXGISurface(ref m_native, ref queueHelperStruct);
                if (FAILED(hr))
                    return;

                // Tracks whether this frame's content was produced. If the user render callback
                // throws we must NOT abort here: with the shared surface queue that would strand
                // the just-dequeued surface outside the queue, and the next Dequeue (which waits
                // INFINITE) would block the UI thread forever. Instead we skip *presenting* this
                // frame (the previous back buffer stays on screen) but still flow the surface back
                // through the queue below so the pipeline keeps running.
                bool rendered = true;
                if (renderMode == QueueRenderMode.RenderDXGI)
                {
                    try
                    {
                        RenderToDXGI(queueHelperStruct.pDXGISurface, isNewSurface);
                    }
                    catch (Exception ex)
                    {
                        // Debug.WriteLine is [Conditional("DEBUG")] so it is stripped in Release;
                        // referencing ex here keeps it "used" (avoids CS0168) without an #if.
                        Debug.WriteLine($"{ex.GetType().FullName}: {ex.Message}, {ex.StackTrace}");
                        rendered = false;
                    }
                }

                hr = NativeWrapper.QueueHelper_GetSurface9(ref m_native, ref queueHelperStruct);
                if (FAILED(hr))
                    return;

                // Only present a frame we actually rendered; on a failed callback we keep the last
                // good back buffer but still enqueue the surface below to recycle it.
                if (rendered)
                    m_d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, queueHelperStruct.pSurface9, true);

                NativeWrapper.QueueHelper_ABProducerEnqueueTexture9(ref m_native, ref queueHelperStruct);
            }
            finally
            {
                try { m_d3dImage.AddDirtyRect(new Int32Rect(0, 0, m_d3dImage.PixelWidth, m_d3dImage.PixelHeight)); } catch { }
                m_d3dImage.Unlock();
                NativeWrapper.QueueHelper_Release(ref queueHelperStruct);
            }
        }
    }
}
