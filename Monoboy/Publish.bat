dotnet publish -c Release -o "../Builds/Windows x64/" -p:PublishSingleFile=true --self-contained false -r win-x64
dotnet publish -c Release -o "../Builds/Windows x86/" -p:PublishSingleFile=true --self-contained false -r win-x86
dotnet publish -c Release -o "../Builds/Linux x64/" -p:PublishSingleFile=true --self-contained false -r linux-x64
dotnet publish -c Release -o "../Builds/Linux x86/" -p:PublishSingleFile=true --self-contained false -r linux-x86