# ByeDPI Manager

[Русский](README.md) | English

A mini utility for running ByeDPI and ProxiFyre.

![Interface Screenshot](screens/screen_en.png)

## Requirements

1. Windows 7+, [.NET Framework 4.7.2+](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net472-offline-installer)
2. [ProxiFyre](https://github.com/wiresock/proxifyre), [Windows Packet Filter](https://github.com/wiresock/ndisapi), [Visual C++ Redist 2022](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version)
3. [ByeDPI](https://github.com/hufrea/byedpi)

## Installation

* Instructions from the community [ByeDPI Manager Manual](https://github.com/BDManual/ByeDPIManager-Manual)

### Option 1: All-in-One (Recommended for beginners)

This option includes all necessary components in a single archive.

1. **Download:**

   * Go to the release page: [https://github.com/romanvht/ByeDPIManager/releases/latest](https://github.com/romanvht/ByeDPIManager/releases/latest)
   * Download the `All_In_One_w64.zip` file

2. **Extraction:**

   * Locate the downloaded file on your computer
   * Right-click and select “Extract All…”
   * Choose an installation folder (e.g., `C:\APPS\ByeDPIManager`)

3. **Installing dependencies:**

   * Open the `redist` folder inside the extracted archive
   * Install both apps from this folder:

     * Windows Packet Filter (required for ProxiFyre)
     * Visual C++ Redistributable 2022

### Option 2: Manual Installation (For advanced users)

If you prefer managing components separately:

1. **Download components separately:**

   * [Manager](https://github.com/romanvht/ByeDPIManager/releases/latest)
   * [ByeDPI](https://github.com/hufrea/byedpi)
   * [ProxiFyre](https://github.com/wiresock/proxifyre)

2. **Install dependencies:**

   * [Windows Packet Filter](https://github.com/wiresock/ndisapi) (Required for ProxiFyre)
   * [Visual C++ Redistributable 2022](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version)

3. **Extract all components into convenient folders**

4. **Run and set paths:**

   * Specify the correct path to `ciadpi.exe` in the ByeDPI tab
   * Specify the correct path to `proxifyre.exe` in the ProxiFyre tab

## Configuration

### Initial Setup

1. **Launch the program:**

   * Run `ByeDPI Manager.exe`
   * Click the "Settings" button

2. **ProxiFyre setup:**

   * Go to the “ProxiFyre” tab
   * Specify applications to be bypassed (e.g., Chrome, Firefox, etc.)

### Strategy Configuration

#### Using a predefined strategy

* Enter the desired strategy in the “Arguments” field on the “ByeDPI” tab

#### Strategy testing (optional)

If you don’t have a predefined strategy, you can use the built-in tester:

1. **Go to the tester tab:**

   * Open the “Strategy Test (Beta)” tab

2. **Start test:**

   * Click “Start”
   * The first time you run it, you’ll be asked to allow `ciadpi.exe` network access – click “Allow”

3. **Select a strategy:**

   * After the test completes, strategies with over 50% success will be listed in the log
   * Select the best one and copy it (Ctrl+C)

4. **Apply the strategy:**

   * Go back to the “ByeDPI” tab
   * Paste the copied strategy into the “Arguments” field (Ctrl+V)

5. **Customize testing (optional):**

   * Edit the files in the `proxytest` folder:

     * `sites.txt` – add your own sites to test
     * `cmds.txt` – add your own strategies to check

### Launch and Test

1. **Activate:**

   * In the main window, click “Connect”
   * The first time, ProxiFyre will ask for network access – click “Allow”

2. **Verify it’s working:**

   * Open a browser or app you configured
   * Check if the resources are accessible

## Troubleshooting

* If the app won’t start, ensure .NET Framework 4.7.2+ is installed
* If bypassing doesn’t work, try a different strategy
* If connection issues occur, ensure Windows Packet Filter is installed properly
* Make sure your antivirus or firewall isn’t blocking the app

## Special Thanks

* [ByeDPI](https://github.com/hufrea/byedpi)
* [ProxiFyre](https://github.com/wiresock/proxifyre)
* [Windows Packet Filter](https://github.com/wiresock/ndisapi)
* [SocksSharp](https://github.com/extremecodetv/SocksSharp)
