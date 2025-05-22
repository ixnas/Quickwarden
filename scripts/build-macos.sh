#!/bin/sh

GIT_VERSION=$(git describe --tags)

echo "-- Clean previous distribution output."
rm -rf ../dist

cd ../src/Quickwarden.UI

echo "-- Build Native AOT binaries."
dotnet clean
dotnet publish -r osx-x64 -c Release -p:DebugType=None -p:DebugSymbols=false
dotnet publish -r osx-arm64 -c Release -p:DebugType=None -p:DebugSymbols=false

echo "-- Create app bundle folder structures."
mkdir -p ../../dist/quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/MacOS
mkdir -p ../../dist/quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/Resources

mkdir -p ../../dist/quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/MacOS
mkdir -p ../../dist/quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/Resources

echo "-- Copy binaries into app bundles."
cp bin/Release/net9.0/osx-x64/publish/* ../../dist/quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/MacOS/
cp bin/Release/net9.0/osx-arm64/publish/* ../../dist/quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/MacOS/

cd ../../

echo "-- Copy Info.plist metadata into app bundles."
cp assets/Info.plist dist/quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/
cp assets/Info.plist dist/quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/

echo "-- Copy icon into app bundles."
cp assets/Icon.icns dist/quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/Resources/
cp assets/Icon.icns dist/quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/Resources/

cd dist/

echo "-- Make a copy excluding the Bitwarden CLI tool."
cp -r quickwarden-${GIT_VERSION}-macos-arm64 quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli
cp -r quickwarden-${GIT_VERSION}-macos-x64 quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli

echo "-- Download Bitwarden CLI executable into app bundles."
curl -L -o bw.zip "https://github.com/bitwarden/clients/releases/download/cli-v2025.4.0/bw-oss-macos-2025.4.0.zip"
unzip bw.zip
rm -f bw.zip
mv bw quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/MacOS/

curl -L -o bw.zip "https://github.com/bitwarden/clients/releases/download/cli-v2025.4.0/bw-oss-macos-arm64-2025.4.0.zip"
unzip bw.zip
rm -f bw.zip
mv bw quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/MacOS/

echo "-- Generate DMG images."
hdiutil create -volname "Quickwarden" -srcfolder quickwarden-${GIT_VERSION}-macos-x64 -ov -format UDZO quickwarden-${GIT_VERSION}-macos-x64.dmg
hdiutil create -volname "Quickwarden" -srcfolder quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli -ov -format UDZO quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli.dmg
hdiutil create -volname "Quickwarden" -srcfolder quickwarden-${GIT_VERSION}-macos-arm64 -ov -format UDZO quickwarden-${GIT_VERSION}-macos-arm64.dmg
hdiutil create -volname "Quickwarden" -srcfolder quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli -ov -format UDZO quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli.dmg

echo "-- Cleanup."
rm -rf quickwarden-${GIT_VERSION}-macos-x64
rm -rf quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli
rm -rf quickwarden-${GIT_VERSION}-macos-arm64
rm -rf quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli
