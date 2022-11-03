using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace TqkLibrary.Wpf.Interop.DirectX
{
    public class D3D11Image : D3DImage
    {
        #region Static
        public static readonly DependencyProperty OnRenderProperty
            = DependencyProperty.Register(
                nameof(OnRender),
                typeof(Action<IntPtr, bool>),
                typeof(D3D11Image),
                new UIPropertyMetadata(null, RenderChanged));
        static void RenderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is D3D11Image d3D11Image && args.NewValue is Action<IntPtr, bool> action)
            {
                if (d3D11Image.Helper != null) d3D11Image.Helper.RenderD2D = action;
            }
        }

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
                if (d3D11Image.Helper != null) d3D11Image.Helper.HWND = hwnd;
            }
        }
        #endregion

        public Action<IntPtr, bool> OnRender
        {
            get { return (Action<IntPtr, bool>)GetValue(OnRenderProperty); }
            set { SetValue(OnRenderProperty, value); }
        }
        public IntPtr WindowOwner
        {
            get { return (IntPtr)GetValue(WindowOwnerProperty); }
            set { SetValue(WindowOwnerProperty, value); }
        }

        public D3D11Image()
        {

        }
        ~D3D11Image()
        {
            this.Helper?.Dispose();
        }

        public void RequestRender()
        {
            this.EnsureHelper();

            // Don't bother with a call if there's no callback registered.
            this.Helper.RequestRenderD2D();
        }
        public void SetPixelSize(int pixelWidth, int pixelHeight)
        {
            this.EnsureHelper();
            this.Helper.SetPixelSize((uint)pixelWidth, (uint)pixelHeight);
        }

        internal SurfaceQueueInteropHelper Helper;
        protected override Freezable CreateInstanceCore()
        {
            return new D3D11Image();
        }
        void EnsureHelper()
        {
            if (this.Helper is null)
            {
                this.Helper = new SurfaceQueueInteropHelper();
                this.Helper.HWND = this.WindowOwner;
                this.Helper.D3DImage = this;
                this.Helper.RenderD2D = this.OnRender;
            }
        }
    }
}
