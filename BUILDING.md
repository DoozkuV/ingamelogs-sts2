# Building and running locally

## Requirements

- .NET SDK 9
- MegaDot 4.5.1, Mega Crit's Godot fork, for exporting the `.pck`. The game refuses packs exported by a newer Godot.
- Slay the Spire 2 installed via Steam

## Configuration

`Sts2PathDiscovery.props` auto-detects the game's install location per OS. Override it by setting `Sts2Path` in `Directory.Build.props` if yours is elsewhere.

`GodotPath` in `Directory.Build.props` must point at the MegaDot executable. The default is the Arch `megadot` package location, `/usr/lib/megadot/MegaDot_v4.5.1-stable_mono_linux.x86_64`.

## Build and install

```sh
dotnet build     # code-only changes: copies the dll + manifest into <game>/mods/InGameLogs
dotnet publish   # also imports the Godot project and exports InGameLogs.pck there
```

`dotnet build` is enough after editing C#. Anything under `InGameLogs/` (the scene or theme) needs `dotnet publish`, which runs MegaDot headless to import assets and export the pack.

Then enable the mod under Settings > Mod Settings in game. If you also subscribe to the Workshop version, the game disables the Workshop copy while a local mod with the same ID exists.

## Layout

```
InGameLogs/         Godot assets shipped in the .pck (scene + theme)
InGameLogsCode/     C# (ModEntry, the NLogViewer scene script, Harmony patches)
fonts/, themes/     editor-only mirrors of the game's console font, excluded from export
project.godot       Godot project root (open this folder in MegaDot to edit the scene)
export_presets.cfg  the "BasicExport" preset used by dotnet publish
```

The scene references the game's own font resource paths (`res://themes/source_code_pro_medium_shared.tres`). The `fonts/` and `themes/` folders mirror those paths so the Godot editor can resolve them. The export preset excludes them, so in game the mod uses the game's copies and never overwrites them. The same trick works for any other game asset you want to reference from a mod scene.

## Notes for scene-attached scripts

`NLogViewer.cs` is bound to the scene root by Godot's source generator, which requires the file name to match the class name. Keep that class free of game (`sts2`) and Harmony types in its signatures, because the MegaDot editor loads the mod assembly when importing and exporting, and it has no access to the game's DLLs. Game-facing calls belong in `ModEntry`.
