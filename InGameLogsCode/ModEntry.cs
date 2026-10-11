using Godot;
using HarmonyLib;
using InGameLogs.Nodes;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;
using LogType = MegaCrit.Sts2.Core.Logging.LogType;
using Log = MegaCrit.Sts2.Core.Logging.Log;
using LogLevel = MegaCrit.Sts2.Core.Logging.LogLevel;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Debug;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
	public const string ModId = "InGameLogs";
	public const string ViewerScenePath = "res://InGameLogs/scenes/log_viewer.tscn";

	public static Logger Logger { get; } = new(ModId, LogType.Generic);

	/// <summary>The log viewer panel once its scene has been added to the game. Null until then.</summary>
	public static NLogViewer? Viewer { get; private set; }

	public static void Initialize()
	{
		// Lets Godot bind scene-attached scripts (NLogViewer.cs) to classes in this assembly.
		// Must run before the scene is loaded.
		Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(typeof(ModEntry).Assembly);

		new Harmony("doozku.ingamelogs").PatchAll(typeof(ModEntry).Assembly);

		AddViewerToGame();
		Logger.Info("loaded");
	}

	private static void AddViewerToGame()
	{
		NGame? game = NGame.Instance;
		if (game == null)
		{
			Logger.Error("NGame.Instance is null during mod init; log viewer not added.");
			return;
		}

		var layer = GD.Load<PackedScene>(ViewerScenePath).Instantiate<CanvasLayer>();
		NLogViewer viewer = layer.GetNode<NLogViewer>("LogViewer");
		Viewer = viewer;

		// Defer so we never mutate the tree while the game is still inside its own _Ready.
		game.CallDeferred(Node.MethodName.AddChild, layer);

		// Debug/VeryDebug toggles start hidden in the scene; reveal them if the game is
		// configured to emit those levels (via -log args or the console's `log` command).
		// The viewer also reveals a toggle itself the first time a line of that level arrives.
		foreach (LogLevel level in new[] { LogLevel.Debug, LogLevel.VeryDebug })
		{
			if (GameEmits(level))
				viewer.CallDeferred(nameof(NLogViewer.ShowLevelToggle), (int)level);
		}

		// The viewer and the game's console are two pages of one overlay: showing the viewer
		// hides the console. (The reverse direction is a Harmony prefix on ShowConsole.)
		viewer.VisibilityChanged += () =>
		{
			if (viewer.Visible)
				NDevConsole.Instance.HideConsole();
		};

		// Log.LogCallback can fire on any thread and runs inside the game's logger lock,
		// so do nothing here except hop to the main thread.
		Log.LogCallback += (level, text, _) =>
			viewer.CallDeferred(nameof(NLogViewer.AppendLine), (int)level, text);
	}

	/// <summary>True if any log type's threshold lets <paramref name="level"/> through (see Logger.WillLog).</summary>
	private static bool GameEmits(LogLevel level)
	{
		LogLevel threshold = Logger.GlobalLogLevel;
		foreach (LogLevel typeLevel in Logger.logLevelTypeMap.Values)
		{
			if (typeLevel < threshold)
				threshold = typeLevel;
		}
		return level >= threshold;
	}
}
