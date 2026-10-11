using MegaCrit.Sts2.Core.Logging;

namespace InGameLogs.Logging; 

public class LogPrinterMux(params ILogPrinter[] printers) : ILogPrinter 
{
	public void Print(LogLevel logLevel, string text, int skipFrames)
	{
		skipFrames++;
		foreach (var printer in printers) {
			printer.Print(logLevel, text, skipFrames);
		}
	}
}
