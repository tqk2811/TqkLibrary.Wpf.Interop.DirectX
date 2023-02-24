# TqkLibrary.Wpf.Interop.DirectX
[WPFDXInterop](https://github.com/microsoft/WPFDXInterop) but P/Invoke version for Net5 or higher.  
Get it on [Nuget](https://www.nuget.org/packages/TqkLibrary.Wpf.Interop.DirectX/) or [Release](https://github.com/tqk2811/TqkLibrary.Wpf.Interop.DirectX/releases)  

______________________
# Sample code
Same as [WPFDXInterop](https://github.com/microsoft/WPFDXInterop) but different namespace.

```xaml
<Window x:Class="WpfApplication.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:WpfApplication"
        mc:Ignorable="d"
        xmlns:DXExtensions="clr-namespace:TqkLibrary.Wpf.Interop.DirectX;assembly=TqkLibrary.Wpf.Interop.DirectX"
        Title="MainWindow" Height="350" Width="525">
    <Grid>
        <Image>
            <Image.Source>
                <DXExtensions:D3D11Image  x:Name="InteropImage"/>
            </Image.Source>
        </Image>
    </Grid>
</Window>
```
Note: Make sure set your project build target to `x86` or `x64`   
Require: C++ 14 (install [vc_redist](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist) 2015 x86/x64 or higher)  

# LICENSE
MIT