cd Monoboy

dotnet publish -r win-x86 -o "../Builds/Windows x86/" --self-contained=false -c Release /p:PublishSingleFile=true
robocopy Data "../Builds/Windows x86/Data" /e

dotnet publish -r win-x64 -o "../Builds/Windows x64/" --self-contained=false -c Release /p:PublishSingleFile=true
robocopy Data "../Builds/Windows x64/Data" /e

pause