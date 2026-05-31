# Steam File Copier

A simple utility for copying `.lua` and `.manifest` files into Steam folders.

## 📋 Purpose

Automatically sorts and copies:
- `.lua` files → `C:\Program Files (x86)\Steam\config\stplug-in`
- `.manifest` files → `C:\Program Files (x86)\Steam\depotcache`

Useful for various Steam client cracks and modifications.

## 🚀 Usage

1. Download **SteamFileCopier.exe** from the [Releases](https://github.com/Gwynbbleidd/steam-file-copier/releases) section
2. Run it with a double click
3. Select the source folder with your files
4. Click **"Start Copying"**

> **Note:** Copying to `Program Files` may require running as Administrator.

## 🔧 Features

- Custom source and destination folder selection
- Duplicate skipping (does not overwrite existing files)
- Recursive file search across all subfolders
- Log box showing the copying process
- Progress bar

## 🛠 Building from Source

Requires: .NET Framework 4.x (included with Windows by default)

```cmd
csc.exe /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /out:release\SteamFileCopier.exe src\SteamFileCopier.cs
```

Or open `src\SteamFileCopier.cs` in Visual Studio / SharpDevelop and build the project.

## 📄 License

MIT License — free to use, modify, and distribute.
