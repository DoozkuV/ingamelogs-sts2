using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug;

namespace InGameLogs.Patches;

/// <summary>
/// The game's console toggles on the backtick keycode and ignores modifiers, so Ctrl+` would open
/// it alongside the log viewer. Skip the console's input handler for that chord.
/// </summary>
[HarmonyPatch(typeof(NDevConsole), nameof(NDevConsole._Input))]
internal static class DevConsoleIgnoresCtrlBacktickPatch
{
	static bool Prefix(InputEvent inputEvent)
	{
		return inputEvent is not InputEventKey { Keycode: Key.Quoteleft } key
			|| !key.IsCommandOrControlPressed();
	}
}

/// <summary>
/// The game only suppresses its hotkeys while its own console is visible
/// (NHotkeyManager._UnhandledInput checks NDevConsole.Instance.Visible). Do the same for the viewer
/// so keys like "end turn" don't fire while reading logs.
/// </summary>
[HarmonyPatch(typeof(NHotkeyManager), nameof(NHotkeyManager._UnhandledInput))]
internal static class HotkeysBlockedWhileViewerOpenPatch
{
	static bool Prefix()
	{
		return ModEntry.Viewer is not { Visible: true };
	}
}

/// <summary>
/// The reverse of the VisibilityChanged hook in ModEntry: opening the game's console hides the
/// viewer, so the two never overlap.
/// </summary>
[HarmonyPatch(typeof(NDevConsole), nameof(NDevConsole.ShowConsole))]
internal static class ViewerHiddenWhenConsoleOpensPatch
{
	static void Prefix()
	{
		ModEntry.Viewer?.HideViewer();
	}
}
