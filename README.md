# In-Game Logs

A Slay the Spire 2 mod that shows the game's log output in-game, in a panel styled after the built-in developer console.

- **Ctrl+`** toggles the log viewer. **Escape** closes it, **F11** toggles fullscreen.
- Filter by level (Load, Info, Warn, Error; Debug levels appear when the game emits them), autoscroll, and clear.
- The viewer and the game's own console (backtick) are two pages of one overlay: opening either hides the other.
- Game hotkeys are suppressed while the viewer is open, the same way the console does it.

Logs are captured through the game's public `Log.LogCallback` event, so no game code is patched to read them. The only Harmony patches are small input-handling prefixes on the dev console and hotkey manager.

Building from source and editing the scene are covered in [BUILDING.md](BUILDING.md).

## License

MIT. See [LICENSE](LICENSE). The bundled editor-preview font is Source Code Pro under the SIL Open Font License, see `fonts/OFL.txt`.
