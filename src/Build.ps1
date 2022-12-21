Remove-Item -Recurse -Force .\x64\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\x86\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\TqkLibrary.Wpf.Interop.DirectX\bin\Release\** -ErrorAction SilentlyContinue

$env:PATH="$($env:PATH);C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE"
devenv .\TqkLibrary.Wpf.Interop.DirectX.sln /Rebuild 'Release|x64' /Project TqkLibrary.Wpf.Interop.DirectX.Native
devenv .\TqkLibrary.Wpf.Interop.DirectX.sln /Rebuild 'Release|x86' /Project TqkLibrary.Wpf.Interop.DirectX.Native
dotnet build --no-incremental .\TqkLibrary.Wpf.Interop.DirectX\TqkLibrary.Wpf.Interop.DirectX.csproj -c Release
nuget pack .\TqkLibrary.Wpf.Interop.DirectX\TqkLibrary.Wpf.Interop.DirectX.nuspec -Symbols -OutputDirectory .\TqkLibrary.Wpf.Interop.DirectX\bin\Release

$nugetKey=$env:nugetKey
if([string]::IsNullOrEmpty($nugetKey))
{
	Write-Host "Build & pack success"
	pause
}
else
{
	Write-Host "enter to push nuget"
	pause
	Write-Host "enter to confirm"
    pause

	$files = [System.IO.Directory]::GetFiles(".\TqkLibrary.Wpf.Interop.DirectX\bin\Release\")
	nuget push $($files[0]) -ApiKey $($nugetKey) -Source "https://api.nuget.org/v3/index.json"
}
pause