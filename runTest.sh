#!/bin/bash
git clone https://github.com/VivianaDuenas/runtimelab --single-branch --branch inflate_improvements
pushd runtimelab/src/Microsoft.ManagedZLib/benchmarks
killall -9 dotnet 2> /dev/null
curl -L -o ./dotnet-install.sh https://dot.net/v1/dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh -Channel 8.0 -InstallDir .
chmod +x ./dotnet
./dotnet run -c Release --filter "*"
cp -r BenchmarkDotNet.Artifacts $HELIX_WORKITEM_UPLOAD_ROOT
popd
killall -9 dotnet 2> /dev/null
rm -rf runtimelab
