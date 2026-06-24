using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace TqkLibrary.Wpf.Interop.DirectX
{
    /// <summary>
    /// Callback invoked to draw a frame into the shared DXGI surface.
    /// </summary>
    /// <param name="IDXGISurface">
    /// Native pointer to the <c>IDXGISurface</c> to render into. It is valid only for the
    /// duration of the callback — do not store it for later use.
    /// </param>
    /// <param name="isNewSurface">
    /// <see langword="true"/> when the surface was just (re)created and must be fully
    /// (re)initialized by the renderer; <see langword="false"/> for an existing surface.
    /// </param>
    /// <remarks>
    /// The callback must not throw. If it does, the library catches the exception, skips that
    /// frame and continues — the exception is only logged in DEBUG builds and is not surfaced
    /// to the caller. Handle your own errors inside the callback.
    /// </remarks>
    public delegate void OnRenderDelegate(IntPtr IDXGISurface, bool isNewSurface);

    /// <summary>
    /// A <see cref="D3DImage"/> whose content is produced by the native DirectX interop
    /// pipeline (a P/Invoke port of Microsoft.Wpf.Interop.DirectX).
    /// <para>
    /// This type owns unmanaged DirectX resources and implements <see cref="IDisposable"/>.
    /// WPF never disposes an <see cref="System.Windows.Media.ImageSource"/> automatically, so
    /// you must call <see cref="Dispose"/> yourself, on the UI thread, when you are finished
    /// with the control — for example from the hosting window's <c>Closing</c> or the host
    /// element's <c>Unloaded</c> handler. If <see cref="Dispose"/> is never called, the native
    /// resources are only released later by a finalizer running on the GC thread.
    /// </para>
    /// </summary>
    public class D3D11Image : D3DImage, IDisposable
    {
        #region Static
        /// <summary>
        /// Identifies the <see cref="OnRender"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OnRenderProperty
            = DependencyProperty.Register(
                nameof(OnRender),
                typeof(OnRenderDelegate),
                typeof(D3D11Image),
                new UIPropertyMetadata(null, RenderChanged));
        static void RenderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is D3D11Image d3D11Image && args.NewValue is OnRenderDelegate onRenderDelegate)
            {
                if (d3D11Image._helper != null) d3D11Image._helper.RenderD2D = onRenderDelegate;
            }
        }
        /// <summary>
        /// Identifies the <see cref="WindowOwner"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowOwnerProperty
            = DependencyProperty.Register(
                nameof(WindowOwner),
                typeof(IntPtr),
                typeof(D3D11Image),
                new UIPropertyMetadata(IntPtr.Zero, HWNDOwnerChanged));
        static void HWNDOwnerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is D3D11Image d3D11Image && args.NewValue is IntPtr hwnd)
            {
                if (d3D11Image._helper != null) d3D11Image._helper.HWND = hwnd;
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the callback used to render each frame into the shared DXGI surface.
        /// The callback must not throw — see <see cref="OnRenderDelegate"/>.
        /// </summary>
        public OnRenderDelegate OnRender
        {
            get { return (OnRenderDelegate)GetValue(OnRenderProperty); }
            set { SetValue(OnRenderProperty, value); }
        }
        /// <summary>
        /// Gets or sets the HWND of the window that owns the rendering; it is used when creating
        /// the underlying D3D9 device.
        /// </summary>
        public IntPtr WindowOwner
        {
            get { return (IntPtr)GetValue(WindowOwnerProperty); }
            set { SetValue(WindowOwnerProperty, value); }
        }

        private SurfaceQueueInteropHelper _helper;
        /// <summary>
        /// Initializes a new instance of the <see cref="D3D11Image"/> class.
        /// </summary>
        public D3D11Image()
        {

        }
        /// <summary>
        /// Releases the native DirectX resources held by this control. Call this on the UI
        /// thread that created the control for deterministic cleanup; otherwise the helper's
        /// own finalizer frees them on the GC thread (with a process-shutdown guard).
        /// <para>
        /// WPF does not call this automatically — see the remarks on <see cref="D3D11Image"/>.
        /// </para>
        /// </summary>
        public void Dispose()
        {
            SurfaceQueueInteropHelper helper = this._helper;
            this._helper = null;
            helper?.Dispose();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Requests that a single frame be rendered now through the <see cref="OnRender"/>
        /// callback. Must be called on the UI thread.
        /// </summary>
        public void RequestRender()
        {
            this.EnsureHelper();

            // Don't bother with a call if there's no callback registered.
            this._helper.RequestRenderD2D();
        }

        /// <summary>
        /// Sets the pixel size of the DirectX render surface. Call on the UI thread, e.g. when
        /// the host element's size changes. Passing a new size recreates the shared surface.
        /// </summary>
        /// <param name="pixelWidth">Desired surface width, in pixels.</param>
        /// <param name="pixelHeight">Desired surface height, in pixels.</param>
        public void SetPixelSize(int pixelWidth, int pixelHeight)
        {
            this.EnsureHelper();
            this._helper.SetPixelSize((uint)pixelWidth, (uint)pixelHeight);
        }

        /// <summary>
        /// When implementing <see cref="Freezable"/>, creates a new instance of
        /// <see cref="D3D11Image"/>.
        /// </summary>
        /// <returns>A new <see cref="D3D11Image"/> instance.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new D3D11Image();
        }
        void EnsureHelper()
        {
            if (this._helper is null)
            {
                this._helper = new SurfaceQueueInteropHelper();
                this._helper.HWND = this.WindowOwner;
                this._helper.D3DImage = this;
                this._helper.RenderD2D = this.OnRender;
            }
        }
    }
}
