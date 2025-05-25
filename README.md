# Quickwarden

Quickly search through your Bitwarden vaults.

<img src="assets/Icon-sm-half.png" width="218" alt="Logo"/>

**Features**
- Instantly search from any application using a global keyboard shortcut
- Search through multiple accounts at once
- Navigate quickly using only your keyboard
- Encrypts your Bitwarden session keys using Windows Credential Vault or macOS Keychain

## Contents
- [Installing a release](#installing-a-release)
- [Usage](#usage)
- [Building from source](#building-from-source)
  - [Development](#development)
  - [Packaging](#packaging)
- [Use of Bitwarden CLI](#use-of-bitwarden-cli)

## Installing a release

Every release is built for the following platforms:

- Windows x64
- macOS arm64 (Apple Silicon)
- macOS x64 (Intel)

Go to the [releases page](https://github.com/ixnas/Quickwarden/releases) and download the latest release for your platform.

**Windows**: Run the installer.

**macOS**: Open the disk image and copy the application to your Applications folder.

There are no external dependencies because Bitwarden CLI is bundled with these distributions.

You may also use the distribution that doesn't contain the Bitwarden CLI.
In that case, you'll have to install Bitwarden CLI yourself, and make sure that its installation directory is included in your `PATH` environment variable.

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