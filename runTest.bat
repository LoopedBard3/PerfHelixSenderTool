@echo on
git clone https://github.com/VivianaDuenas/runtimelab --single-branch --branch inflate_improvements
pushd runtimelab/src/Microsoft.ManagedZLib/benchmarks
TASKKILL /F /T /IM dotnet.exe 2> nul
Invoke-WebRequest -URI https://dot.net/v1/dotnet-install.ps1 -OutFile ./dotnet-install.ps1
./dotnet-install.ps1 -Channel 8.0 -InstallDir .
dotnet run -c Release --filter * --minIterationCount 1 --maxIterationCount 2
xcopy BenchmarkDotNet.Artifacts $env:HELIX_WORKITEM_UPLOAD_ROOT
popd
TASKKILL /F /T /IM dotnet.exe 2> nul
Remove-Item -Recurse -Force runtimelab