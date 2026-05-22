namespace Monoboy.Desktop;

using System;
using System.Linq;

public class Program
{
    public static void Main()
    {
        string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        if (CliHelp.WantsHelp(args))
        {
            CliHelp.Print();
            return;
        }

        if (args.Contains("--test"))
        {
            TestDebugger.Run(args);
            return;
        }

        if (args.Contains("--debug"))
        {
            TuiDebugger.Run(args);
            return;
        }

        Application app = new();
        app.Run();
    }
}
