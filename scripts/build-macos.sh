#!/bin/sh

GIT_VERSION=$(git describe --tags --always)
GIT_LAST_TAG=$(git describe --tags --always --abbrev=0)

echo "-- Clean previous distribution output."
rm -rf ../dist

cd ../src/Quickwarden.UI

echo "-- Build Native AOT binaries."
dotnet clean
dotnet publish -r osx-x64 -c Release -p:DebugType=None -p:DebugSymbols=false -p:Version=${GIT_LAST_TAG} -p:AssemblyVersion=${GIT_LAST_TAG} -p:InformationalVersion=${GIT_LAST_TAG}
dotnet publish -r osx-arm64 -c Release -p:DebugType=None -p:DebugSymbols=false -p:Version=${GIT_LAST_TAG} -p:AssemblyVersion=${GIT_LAST_TAG} -p:InformationalVersion=${GIT_LAST_TAG}

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
sed -e "s/%VERSION%/${GIT_LAST_TAG}/g" assets/Info.plist > dist/quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/Info.plist
sed -e "s/%VERSION%/${GIT_LAST_TAG}/g" assets/Info.plist > dist/quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/Info.plist

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

echo "-- Copy license files."
cp ../LICENSE.txt quickwarden-${GIT_VERSION}-macos-x64/License.txt
cp ../LICENSE-BW.txt "quickwarden-${GIT_VERSION}-macos-x64/License for Bitwarden CLI.txt"
cp ../LICENSE.txt quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli/License.txt

cp ../LICENSE.txt quickwarden-${GIT_VERSION}-macos-arm64/License.txt
cp ../LICENSE-BW.txt "quickwarden-${GIT_VERSION}-macos-arm64/License for Bitwarden CLI.txt"
cp ../LICENSE.txt quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli/License.txt

echo "-- Sign app bundles"
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/MacOS/*.dylib
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/MacOS/*.dylib
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli/Quickwarden.app/Contents/MacOS/*.dylib
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli/Quickwarden.app/Contents/MacOS/*.dylib

codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/MacOS/bw
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/MacOS/bw

codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/MacOS/Quickwarden.UI
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/MacOS/Quickwarden.UI
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli/Quickwarden.app/Contents/MacOS/Quickwarden.UI
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli/Quickwarden.app/Contents/MacOS/Quickwarden.UI

codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/Info.plist
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/Info.plist
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli/Quickwarden.app/Contents/Info.plist
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli/Quickwarden.app/Contents/Info.plist

codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app/Contents/Resources/*
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app/Contents/Resources/*
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli/Quickwarden.app/Contents/Resources/*
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli/Quickwarden.app/Contents/Resources/*

codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64/Quickwarden.app
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64/Quickwarden.app
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli/Quickwarden.app
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli/Quickwarden.app

echo "-- Generate DMG images."
hdiutil create -volname "Quickwarden" -srcfolder quickwarden-${GIT_VERSION}-macos-x64 -ov -format UDZO quickwarden-${GIT_VERSION}-macos-x64.dmg
hdiutil create -volname "Quickwarden" -srcfolder quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli -ov -format UDZO quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli.dmg
hdiutil create -volname "Quickwarden" -srcfolder quickwarden-${GIT_VERSION}-macos-arm64 -ov -format UDZO quickwarden-${GIT_VERSION}-macos-arm64.dmg
hdiutil create -volname "Quickwarden" -srcfolder quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli -ov -format UDZO quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli.dmg

echo "-- Sign DMG images"
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64.dmg
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64.dmg
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli.dmg
codesign -fs "Sjoerd Scheffer" quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli.dmg

echo "-- Cleanup."
rm -rf quickwarden-${GIT_VERSION}-macos-x64
rm -rf quickwarden-${GIT_VERSION}-macos-x64-no-bw-cli
rm -rf quickwarden-${GIT_VERSION}-macos-arm64
rm -rf quickwarden-${GIT_VERSION}-macos-arm64-no-bw-cli
