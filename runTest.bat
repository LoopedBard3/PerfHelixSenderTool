@echo on
git clone https://github.com/VivianaDuenas/runtimelab --single-branch --branch inflate_improvements
pushd runtimelab\src\Microsoft.ManagedZLib\benchmarks
TASKKILL /F /T /IM dotnet.exe 2> nul
powershell -Command "Invoke-WebRequest -URI https://dot.net/v1/dotnet-install.ps1 -OutFile ./dotnet-install.ps1"
powershell -NoProfile -ExecutionPolicy Bypass .\dotnet-install.ps1 -Channel 8.0 -InstallDir .
dotnet run -c Release --filter "*" --minIterationCount 1 --maxIterationCount 2
xcopy BenchmarkDotNet.Artifacts %HELIX_WORKITEM_UPLOAD_ROOT%
popd
TASKKILL /F /T /IM dotnet.exe 2> nul
rmdir /s /q runtimelab