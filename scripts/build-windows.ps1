git describe --tags | Tee-Object -Variable GIT_VERSION

echo "-- Clean previous distribution output."
Remove-Item ..\dist -Recurse -Force

cd ..\src\Quickwarden.UI

echo "-- Build Native AOT binaries."
dotnet clean
dotnet publish -r win-x64 -c Release -p:DebugType=None -p:DebugSymbols=false

echo "-- Create application directories."
New-Item -ItemType Directory -Path ..\..\dist\quickwarden-${GIT_VERSION}-windows-x64\build -Force
New-Item -ItemType Directory -Path ..\..\dist\quickwarden-${GIT_VERSION}-windows-x64-no-bw-cli\build -Force

echo "-- Copy binaries into application directories."
cp bin\Release\net9.0\win-x64\publish\* ..\..\dist\quickwarden-${GIT_VERSION}-windows-x64\build\
cp bin\Release\net9.0\win-x64\publish\* ..\..\dist\quickwarden-${GIT_VERSION}-windows-x64-no-bw-cli\build\

cd ..\..\

echo "-- Download Bitwarden CLI executable into application directory."
cd dist\quickwarden-${GIT_VERSION}-windows-x64\build\
curl -o bw.zip "https://github.com/bitwarden/clients/releases/download/cli-v2025.4.0/bw-oss-windows-2025.4.0.zip"
C:\Windows\System32\tar.exe -xf bw.zip
Remove-Item bw.zip
