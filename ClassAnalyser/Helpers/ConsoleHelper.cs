using Spectre.Console;

namespace ClassAnalyser.Helpers
{
    public class ConsoleHelper
    {
        public static T StatusCommon<T>(string in_status, Func<StatusContext, T> in_action)
        {
            return AnsiConsole.Status().Spinner(Spinner.Known.Line).Start(in_status, in_action);
        }

        public static void StatusCommon(string in_status, Action<StatusContext> in_action)
        {
            AnsiConsole.Status().Spinner(Spinner.Known.Line).Start(in_status, in_action);
        }
    }
}
