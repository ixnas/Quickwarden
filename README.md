# Quickwarden
[![macOS build](https://github.com/ixnas/Quickwarden/actions/workflows/macos.yml/badge.svg)](https://github.com/ixnas/Quickwarden/actions/workflows/macos.yml)
[![Windows build](https://github.com/ixnas/Quickwarden/actions/workflows/windows.yml/badge.svg)](https://github.com/ixnas/Quickwarden/actions/workflows/windows.yml)

Quickly search through your Bitwarden vaults.

<img src="assets/Icon-sm-half.png" width="218" alt="Logo"/>

**Features**
- Instantly search from any application using a global keyboard shortcut
- Search through multiple accounts at once
- Navigate quickly using only your keyboard
- Encrypts your Bitwarden session keys using Windows Credential Vault or macOS Keychain

## Contents
- [Installing a release](#installing-a-release)
  - [Windows](#windows)
  - [macOS](#macos)
- [Usage](#usage)
- [Building from source](#building-from-source)
  - [Development](#development)
  - [Packaging](#packaging)
- [Security](#security)
- [Disclaimer](#disclaimer)
- [Contributing](#contributing)
- [Use of Bitwarden CLI](#use-of-bitwarden-cli)

## Installing a release

Every release is built for the following platforms:

- Windows x64
- macOS arm64 (Apple Silicon)
- macOS x64 (Intel)

### Windows

Go to the [releases page](https://github.com/ixnas/Quickwarden/releases) and download the latest installer.
This would be the file that's called `quickwarden-<version>-windows-x64-setup.exe`.

Run the installer.
If a Windows Defender dialog pops up, click "More info" and then click "Run anyway".

### macOS

Go to the [releases page](https://github.com/ixnas/Quickwarden/releases) and download the latest release for your platform:

- For Apple Silicon Macs, download `quickwarden-<version>-macos-arm64.dmg`.
- For Intel-based Macs, download `quickwarden-<version>-macos-x64.dmg`.

Open the disk image by right-clicking it and clicking Open.

Because this application is not signed and notarized by Apple, you'll have to manually allow the downloaded disk image to open.
Open System Settings and go to "Privacy & Security".
Scroll to the Security section and click "Open anyway".

Now you can copy the application to your Applications folder and eject the disk image.
You may have to repeat the steps above when you launch Quickwarden for the first time.

## Usage
When you first start Quickwarden on macOS you might have to grant access to the Accessibility API.
This enables Quickwarden to be shown by using a global shortcut.

After you start Quickwarden, use **Ctrl+Alt+P** on Windows, or **⌘⌥P** on macOS to open the main window.

To set up your Bitwarden account, use **Ctrl+S** on Windows, or **⌘S** on macOS to open the settings window.
From there you can sign in to your Bitwarden account.

You can now use Quickwarden to search through your vault and copy credentials!

## Building from source
### Development
For development, you'll need the following:
- .NET 9 SDK
- Bitwarden CLI (must be in your PATH environment variable)

The source code in the `src` directory consists of the following projects:
- **Quickwarden.UI** contains the application's entry point and the UI code.
- **Quickwarden.Application** contains all the high level logic.
- **Quickwarden.Infrastructure** contains the low-level implementation details.
- **Quickwarden.Tests** contains tests for the high level application logic.

### Packaging
To create a package, run the script for your current platform in the `scripts` directory.
The packages will be created in the `dist` directory.

If you'd like to build installers on Windows, you'll have to install Inno Setup first.

## Security

This application uses Bitwarden CLI under the hood.
When you sign in, Quickwarden will pass your credentials to Bitwarden CLI.
Your credentials won't ever be stored by Quickwarden, they will only be used to obtain a session key from Bitwarden CLI.

This session key is used to access your vault through the Bitwarden CLI.
Quickwarden will load your vault into memory, it will never store any vault data.

Any other data that Quickwarden needs to operate is always stored fully encrypted using AES encryption.
This currently consists of:

 - E-mail addresses for signed-in users
 - Bitwarden CLI vault location and session keys
 - IDs of your most recently used vault items

The key to unlock this data is securely stored in your platform's credential manager.
On Windows Quickwarden will use the Windows Hello, on macOS it'll use Keychain.

The binary releases are currently signed with a self-signed certificate.
This ensures that only these signed binary releases are able to unlock Quickwarden's encrypted data.
Other applications won't be able to access this data unless if you're on macOS and specifically allow another app to access Quickwarden's keychain entry.

## Disclaimer

This software is an independent project and not affiliated with Bitwarden in any way.
Use this software at your own risk.
I am not responsible for any lost or stolen credentials as a result of using this application.

## Contributing

If you have any suggestions for improvements or new features, especially when it comes to security, please open a new issue.
I'll try to get to them as soon as I can.

## Use of Bitwarden CLI

This software, Quickwarden, is released under the BSD 2-Clause License (see LICENSE.txt file). It is an independent project and does not contain or link to any GPLv3-licensed source code.

However, this project invokes an external command-line tool called `bw` (Bitwarden CLI), which is licensed under the GNU General Public License v3 (GPLv3).

We have releases that include an unmodified binary distribution of this GPLv3-licensed tool for convenience.
When you run Quickwarden, it may execute the external program as a subprocess, similar to how a shell script or build system might invoke external tools.

Alternatively, you may choose to not use our distribution that includes this binary.
You can then install Bitwarden CLI yourself from your operating system's package manager or from Bitwarden's official distribution.

To comply with the GPLv3, we are providing:
- A copy of the GPLv3 license ([LICENSE-BW.txt](LICENSE-BW.txt))
- A link to the full source code of the tool:
  - https://github.com/bitwarden/clients