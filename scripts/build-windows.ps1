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

cd dist

echo "-- Fetch Bitwarden clients repository and build Bitwarden CLI executables."
git clone "https://github.com/bitwarden/clients.git" --depth=1 --branch cli-v2025.4.0
cd clients
npm ci
cd apps\cli
npm run build:oss:prod
npm run clean
npx pkg . --targets latest-win-x64 --output .\dist\oss\win-x64\bw.exe

echo "-- Copy Bitwarden CLI executable into application directory."
cp dist\oss\win-x64\bw.exe ..\..\..\quickwarden-${GIT_VERSION}-windows-x64\build\bw.exe

echo "-- Delete Bitwarden clients repository."
cd ..\..\..\
Remove-Item clients -Recurse -Force
