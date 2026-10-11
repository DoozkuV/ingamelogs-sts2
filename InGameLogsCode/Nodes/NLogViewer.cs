using Godot;

namespace InGameLogs.Nodes;

/// <summary>
/// Script for the root Panel of res://InGameLogs/scenes/log_viewer.tscn.
///
/// Notes:
///  - File name must equal the class name so Godot's source generator emits the
///    [ScriptPath("res://InGameLogsCode/Nodes/NLogViewer.cs")] that binds this class to the scene.
///  - Keep this class free of sts2/Harmony types in its signatures so the MegaDot editor
///    can load the assembly headless (needed for --import / --export-pack). Log levels are
///    therefore passed as the game's LogLevel ordinal (int).
/// </summary>
public partial class NLogViewer : Panel
{
    /// <summary>Everything the viewer knows about one log level.</summary>
    private sealed class LevelInfo(string tag, string htmlColor, string togglePath)
    {
        public string Tag { get; } = tag;
        public Color Color { get; } = Color.FromHtml(htmlColor);
        public string TogglePath { get; } = togglePath;

        /// <summary>Lines of this level are rendered. Mirrors the toggle button's pressed state.</summary>
        public bool Enabled { get; set; }

        public Button Toggle { get; set; } = null!;
    }

    // Indexed by the game's LogLevel ordinal: VeryDebug, Load, Debug, Info, Warn, Error.
    // Colours match the game's EditorLogPrinter palette.
    private readonly LevelInfo[] _levels =
    {
        new("VeryDebug", "#B180D3", "%VeryDebugToggle"),
        new("Load", "#87CEEB", "%LoadToggle"),
        new("Debug", "#B180D3", "%DebugToggle"),
        new("Info", "#76FF56", "%InfoToggle"),
        new("Warn", "#FFCB3D", "%WarnToggle"),
        new("Error", "#FF4747", "%ErrorToggle"),
    };

    /// <summary>Oldest entries are dropped once the buffer exceeds this.</summary>
    private const int MaxLines = 5000;

    private readonly record struct Entry(int Level, string Text);

    private RichTextLabel _output = null!;
    private Label _lineCount = null!;

    // Every line received (capped), regardless of filter. The label always shows exactly
    // the entries whose level is enabled, so toggles can rebuild it from here.
    private readonly List<Entry> _entries = new();
    private int _visibleLines;

    private bool _autoscroll = true;
    private bool _isFullscreen;

    public bool IsFullscreen => _isFullscreen;

    public override void _Ready()
    {
        _output = GetNode<RichTextLabel>("%OutputBuffer");
        _lineCount = GetNode<Label>("%LineCount");

        // The scene is the source of truth for each toggle's initial pressed/visible state.
        foreach (LevelInfo level in _levels)
        {
            level.Toggle = GetNode<Button>(level.TogglePath);
            level.Enabled = level.Toggle.ButtonPressed;
            level.Toggle.Toggled += on =>
            {
                level.Enabled = on;
                RebuildOutput();
            };
        }

        MakeHalfScreen();
        UpdateLineCount();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key)
            return;

        // No "is a text field focused" guard here: the Ctrl modifier already prevents
        // accidental triggers, and the guard would block swapping from the console's prompt.
        if (key.Keycode == Key.Quoteleft && key.IsCommandOrControlPressed())
        {
            ToggleViewer();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (!Visible)
            return;
        switch (key.Keycode)
        {
            case Key.Escape:
                HideViewer();
                GetViewport().SetInputAsHandled();
                break;
            case Key.F11:
                ToggleFullscreen();
                GetViewport().SetInputAsHandled();
                break;
        }
    }

    // ---- Public API for the log plumbing ------------------------------------------------

    /// <summary>
    /// Append one log line. <paramref name="level"/> is the game's LogLevel ordinal.
    /// Must be called on the main thread (ModEntry marshals via CallDeferred).
    /// </summary>
    public void AppendLine(int level, string text)
    {
        if (!IsValidLevel(level))
            return;

        // The game is clearly emitting this level, so make sure its filter is offered.
        ShowLevelToggle(level);

        _entries.Add(new Entry(level, text));
        if (_levels[level].Enabled)
        {
            WriteEntry(level, text);
            _visibleLines++;
        }

        if (_entries.Count > MaxLines)
        {
            Entry oldest = _entries[0];
            _entries.RemoveAt(0);
            if (_levels[oldest.Level].Enabled)
            {
                _output.RemoveParagraph(0);
                _visibleLines--;
            }
        }

        UpdateLineCount();
    }

    /// <summary>
    /// Reveal a level's filter toggle. Toggles for levels the game normally doesn't emit
    /// (Debug, VeryDebug) start hidden in the scene.
    /// </summary>
    public void ShowLevelToggle(int level)
    {
        if (IsValidLevel(level))
            _levels[level].Toggle.Visible = true;
    }

    public void ShowViewer()
    {
        Visible = true;
        // Don't let keystrokes keep landing in whatever text field was focused underneath.
        GetViewport()?.GuiReleaseFocus();
    }

    public void HideViewer()
    {
        Visible = false;
        GetViewport()?.GuiReleaseFocus();
    }

    public void ToggleViewer()
    {
        if (Visible)
            HideViewer();
        else
            ShowViewer();
    }

    public void ToggleFullscreen()
    {
        if (_isFullscreen)
            MakeHalfScreen();
        else
            MakeFullScreen();
    }

    /// <summary>Top half of the viewport, like the game's console default.</summary>
    public void MakeHalfScreen()
    {
        AnchorBottom = 0.5f;
        OffsetBottom = 0f;
        _isFullscreen = false;
    }

    /// <summary>Whole viewport.</summary>
    public void MakeFullScreen()
    {
        AnchorBottom = 1.0f;
        OffsetBottom = 0f;
        _isFullscreen = true;
    }

    // ---- Rendering ---------------------------------------------------------------------

    private bool IsValidLevel(int level) => level >= 0 && level < _levels.Length;

    /// <summary>Appends one coloured line to the label. AddText is literal, so brackets are safe.</summary>
    private void WriteEntry(int level, string text)
    {
        LevelInfo info = _levels[level];
        _output.PushColor(info.Color);
        _output.AddText($"[{info.Tag}] ");
        _output.Pop();
        _output.AddText(text + "\n");
    }

    /// <summary>Re-renders the label from the backing list using the current level filter.</summary>
    private void RebuildOutput()
    {
        _output.Clear();
        _visibleLines = 0;
        foreach (Entry e in _entries)
        {
            if (!_levels[e.Level].Enabled)
                continue;
            WriteEntry(e.Level, e.Text);
            _visibleLines++;
        }
        UpdateLineCount();
    }

    private void UpdateLineCount()
    {
        _lineCount.Text =
            _visibleLines == _entries.Count
                ? $"{_entries.Count} lines"
                : $"{_visibleLines} / {_entries.Count} lines";
    }

    // ---- Toolbar signal handlers (connected in log_viewer.tscn) --------------------------

    private void OnAutoscrollToggled(bool on)
    {
        _autoscroll = on;
        _output.ScrollFollowing = on;
    }

    private void OnClearPressed()
    {
        _entries.Clear();
        _output.Clear();
        _visibleLines = 0;
        UpdateLineCount();
    }
}
