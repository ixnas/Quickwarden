git describe --tags --always | Tee-Object -Variable GIT_VERSION
git describe --tags --always --abbrev=0 | Tee-Object -Variable GIT_LAST_TAG

echo "-- Clean previous distribution output."
Remove-Item ..\dist -Recurse -Force

cd ..\src\Quickwarden.UI

echo "-- Build Native AOT binaries."
dotnet clean
dotnet publish -r win-x64 -c Release -p:DebugType=None -p:DebugSymbols=false -p:Version=${GIT_LAST_TAG} -p:AssemblyVersion=${GIT_LAST_TAG} -p:InformationalVersion=${GIT_LAST_TAG}

echo "-- Create application directories."
New-Item -ItemType Directory -Path ..\..\dist\quickwarden-${GIT_VERSION}-windows-x64 -Force
New-Item -ItemType Directory -Path ..\..\dist\quickwarden-${GIT_VERSION}-windows-x64-no-bw-cli -Force

echo "-- Copy binaries into application directories."
cp bin\Release\net9.0\win-x64\publish\* ..\..\dist\quickwarden-${GIT_VERSION}-windows-x64\
cp bin\Release\net9.0\win-x64\publish\* ..\..\dist\quickwarden-${GIT_VERSION}-windows-x64-no-bw-cli\

cd ..\..\

echo "-- Copy licenses into application directories"
cp LICENSE.txt dist\quickwarden-${GIT_VERSION}-windows-x64\
cp LICENSE-BW.txt dist\quickwarden-${GIT_VERSION}-windows-x64\
cp LICENSE.txt dist\quickwarden-${GIT_VERSION}-windows-x64-no-bw-cli\

echo "-- Download Bitwarden CLI executable into application directory."
cd dist\quickwarden-${GIT_VERSION}-windows-x64\
$ProgressPreference = 'SilentlyContinue'
curl -o bw.zip "https://github.com/bitwarden/clients/releases/download/cli-v2025.4.0/bw-oss-windows-2025.4.0.zip"
C:\Windows\System32\tar.exe -xf bw.zip
Remove-Item bw.zip