# reXIP GUI

A Windows GUI for [reXIP](https://github.com/MDashK/reXIP), the command line reader/writer for the
`XIP2` (`.pak`) archives of **DJMAX Online**. It walks you through the same steps as the command line
(get keys → extract → build a patch PAK) without having to type commands.

The UI is available in **English** and **简体中文** (switch under *Settings → Language*).

> **This is an unofficial third-party tool.** It is not affiliated with, endorsed by, or connected to
> Pentavision, Neowiz, or the author of reXIP. See [Disclaimer](#disclaimer).

## Features

- Pick the running DJMAX process and extract the client keys in one click (`dump` + `keys`)
- Or reuse an existing `keyFiles` folder
- Browse, filter, verify and extract entries from `system*.pak`
- Build a new numbered patch PAK from a folder or from single-file mappings, and optionally copy it
  into the game folder (existing files are backed up first)
- Rebuild `system.crc` (advanced)
- Live log of everything the underlying `reXIP.exe` prints, with a cancel button
- Remembers your paths and language between runs

## Requirements

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) to build (a self-contained build needs no runtime)
- `reXIP.exe` from the [reXIP repository](https://github.com/MDashK/reXIP) — **not included here**, see [Upstream](#upstream-rexip)

## Build

```
dotnet publish reXIP.Gui/reXIP.Gui.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

Put `reXIP.exe` into `publish\` next to `reXIP.Gui.exe` and the GUI will pick it up automatically.
You can also point to it manually under *Settings*.

## Usage

1. Start DJMAX and leave it at the "cannot connect to server" pop-up (no login needed).
2. **Get keys** tab → select the DJMax process → *Get keys*.
   (If you already have a `keyFiles` folder, use *Use an existing keyFiles folder…* instead.)
3. In *Settings*, set the game `FILES` folder. On the **Browse / Extract** tab, list the archive and
   extract the files you want to edit into the patch folder.
4. Edit the files, then delete everything you did not change from the patch folder.
5. On the **Build patch** tab, use *Use next number* and then *Build*.

The game loads `system*.pak` files in numeric order and the last one wins, so a new `system_000N.pak`
overrides entries of the original archives without touching them. Always keep backups.

## Project layout

| Path | Purpose |
| --- | --- |
| `reXIP.Gui/CliRunner.cs` | Starts `reXIP.exe` and streams its output (the only contact point between GUI and CLI) |
| `reXIP.Gui/EntryParser.cs` | Parses one line of `reXIP list` output into an in-archive path |
| `reXIP.Gui/Loc.cs` | UI strings for English and Simplified Chinese |
| `reXIP.Gui/MainWindow.xaml(.cs)` | The window and the command wiring |

To add a language, extend the tuples in `Loc.cs`, add an item to the language `ComboBox` in
`MainWindow.xaml`, and handle the new code in `Loc.SetLanguage`.

## Upstream: reXIP

This project is only a front end. All archive handling is done by
[MDashK/reXIP](https://github.com/MDashK/reXIP), which in turn is based on
[ADHSoft/Xip-Pak-Extractor](https://github.com/ADHSoft/Xip-Pak-Extractor). Thanks to both authors.

At the time of writing, reXIP does not publish a license. For that reason this repository does **not**
contain or redistribute reXIP's source code or binaries; download `reXIP.exe` from the upstream
project yourself.

## Disclaimer

- This project ships **no game data, no game code and no encryption keys**. You need your own copy of
  the game, and keys are extracted from your own running client.
- DJMAX Online is © Pentavision / Neowiz. All trademarks belong to their respective owners.
- Modifying game files can stop the game from starting. Back up everything you replace.
- Use at your own risk and only in ways that comply with the game's terms of service and the law in
  your country.

## License

[MIT](LICENSE) — applies to the code in this repository only, not to reXIP or to any game content.
