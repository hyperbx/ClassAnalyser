using ClassAnalyser.Analysis.RTTI;
using ClassAnalyser.Analysis.RTTI.IO;
using ClassAnalyser.Analysis.RTTI.Types;
using ClassAnalyser.Helpers;
using ClassAnalyser.Services;
using Spectre.Console;
using System.Diagnostics;

namespace ClassAnalyser
{
    public class Program
    {
        static void Main()
        {
            var pid  = PromptProcessID();
            var addr = PromptClassAddress(pid);

            Welcome();

            var factory = new RTTIFactory(new RTTIReaderWin32(pid));
            var rtti    = factory.GetRuntimeInfoFromClass(addr);

            if (rtti == null)
            {
                Console.WriteLine($"No class found at 0x{addr:X8} in process {pid}...");
                Thread.Sleep(1500);
                Main();
                return;
            }

            Console.WriteLine($"Object at 0x{addr:X8} in process {pid} is a class with a vftable at 0x{factory.GetVftableFromClass(addr):X8}.");

            var info = ConsoleHelper.StatusCommon("Getting runtime type information...",
                ctx => rtti.GetClassInfo());

            Console.WriteLine($"\r\n{info}");

            PromptExportToHeaders(rtti);
        }

        static void Welcome()
        {
            Console.Clear();
            Console.WriteLine($"Class Analyser [Version 1.0.0] by Hyper\r\n");
        }

        static int PromptProcessID()
        {
            Welcome();

            if (!TryParseProcessID(AnsiConsole.Ask<string>("Process ID or name:"), out var out_processID))
                return PromptProcessID();

            return out_processID;
        }

        static nuint PromptClassAddress(int in_processID)
        {
            Welcome();

            var addr = AnsiConsole.Ask<ulong>("Address:");

            if (!MemoryService.IsAccessible(in_processID, (nuint)addr))
                return PromptClassAddress(in_processID);

            return (nuint)addr;
        }

        static void PromptExportToHeaders(CompleteObjectLocator in_rtti)
        {
            if (!AnsiConsole.Confirm("Export to C++ headers?"))
                return;

            var isExportBaseClasses = AnsiConsole.Confirm("Export base classes?");
            var path = AnsiConsole.Ask<string>("Path:");

            if (path.IsNullOrEmptyOrWhiteSpace())
            {
                PromptExportToHeaders(in_rtti);
                return;
            }

            ConsoleHelper.StatusCommon("Exporting classes...",
                ctx => in_rtti.ExportHeaders(path, isExportBaseClasses, true));

            Console.WriteLine($"\r\nExported classes to \"{path}\".");
        }

        static bool TryParseProcessID(string in_str, out int out_processID)
        {
            if (int.TryParse(in_str, out var _out_processID))
            {
                out_processID = _out_processID;
                return true;
            }

            var processes = Process.GetProcessesByName(in_str);

            if (processes.Length > 1)
            {
                var processIDs = processes.Select(x => x.Id).ToArray();

                Welcome();

                out_processID = AnsiConsole.Prompt(
                    new SelectionPrompt<int>().Title("Which process?").PageSize(15).AddChoices(processIDs));

                return true;
            }

            if (processes.Length == 0)
            {
                out_processID = 0;
                return false;
            }

            out_processID = Process.GetProcessesByName(in_str).First().Id;
            return true;
        }
    }
}