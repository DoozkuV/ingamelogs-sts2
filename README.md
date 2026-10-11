# In-Game Logs

A Slay the Spire 2 mod that shows the game's log output in-game, in a panel styled after the built-in developer console.

- **Ctrl+`** toggles the log viewer. **Escape** closes it, **F11** toggles fullscreen.
- Filter by level (Load, Info, Warn, Error; Debug levels appear when the game emits them), autoscroll, and clear.
- The viewer and the game's own console (backtick) are two pages of one overlay: opening either hides the other.
- Game hotkeys are suppressed while the viewer is open, the same way the console does it.

Logs are captured through the game's public `Log.LogCallback` event, so no game code is patched to read them. The only Harmony patches are small input-handling prefixes on the dev console and hotkey manager.

## Building

Requirements:

- .NET SDK 9
- MegaDot 4.5.1, Mega Crit's Godot fork, for exporting the `.pck`
- Slay the Spire 2 installed via Steam

Set `GodotPath` in `Directory.Build.props` if MegaDot is not at `/usr/lib/megadot/MegaDot_v4.5.1-stable_mono_linux.x86_64`. The game's install location is auto-detected per OS in `Sts2PathDiscovery.props` and can be overridden with `Sts2Path`.

```sh
dotnet build     # code-only changes: copies the dll + manifest into <game>/mods/InGameLogs
dotnet publish   # also imports the Godot project and exports InGameLogs.pck there
```

Enable the mod under Settings > Mod Settings in game.

## Layout

```
InGameLogs/         Godot assets shipped in the .pck (scene + theme)
InGameLogsCode/     C# (ModEntry, the NLogViewer scene script, Harmony patches)
fonts/, themes/     editor-only mirrors of the game's console font, excluded from export
project.godot       Godot project root (open this folder in MegaDot to edit the scene)
```

The scene references the game's own font resource paths. The `fonts/` and `themes/` folders mirror those paths so the Godot editor can resolve them; the export preset excludes them, so in game the mod uses the game's copies.

## License

MIT. See [LICENSE](LICENSE). The bundled editor-preview font is Source Code Pro under the SIL Open Font License, see `fonts/OFL.txt`.
