using HarmonyLib;
using InGameLogs.Logging;
using MegaCrit.Sts2.Core.Logging;

namespace InGameLogs.Patches;

[HarmonyPatch(typeof(Logger), MethodType.Constructor, new[] { typeof(int) })]
internal static class LoggerConstructorPatch
{
    static void Postfix(Logger __instance)
    {
        ref ILogPrinter printer = ref AccessTools.StaticFieldRefAccess<ILogPrinter>(
            typeof(Logger),
            "_logPrinter"
        );

        // Mux is already set
        if (printer is LogPrinterMux)
        {
            return;
        }

        printer = new LogPrinterMux(
            printer,
            printer is ConsoleLogPrinter ? new EditorLogPrinter() : new ConsoleLogPrinter()
        );
    }
}
