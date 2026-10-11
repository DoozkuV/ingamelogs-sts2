using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
	public const string ModId = "InGameLogs";
	public static Logger Logger { get; } = new(ModId, LogType.Generic);

	public static void Initialize()
	{
		new Harmony("doozku.ingamelogs").PatchAll(typeof(ModEntry).Assembly);
		Logger.Info("loaded");
	}
}

