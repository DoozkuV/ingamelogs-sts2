# In-Game Logs

A Slay the Spire 2 mod that shows the game's log output in-game, in a panel styled after the built-in developer console.

- **Ctrl+`** toggles the log viewer. **Escape** closes it, **F11** toggles fullscreen.
- Filter by level (Load, Info, Warn, Error; Debug levels appear when the game emits them), autoscroll, and clear.

Logs are captured through the game's public `Log.LogCallback` event.

Building from source and editing the scene are covered in [BUILDING.md](BUILDING.md).

## License

MIT. See [LICENSE](LICENSE). The bundled editor-preview font is Source Code Pro under the SIL Open Font License, see `fonts/OFL.txt`.
