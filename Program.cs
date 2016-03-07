namespace Pluton.Patcher
{
    using System;
    using System.IO;
    using System.Linq;
    using Pluton.Patcher.Reflection;

    class MainClass
    {
        public static AssemblyPatcher rustAssembly;
        public static string Version { get; } = typeof(MainClass).Assembly.GetName().Version.ToString();

        internal static bool gendiffs = false;
        internal static bool newAssCS = false;

        internal static bool LogToFile = false;
        internal static string LogFile = "Pluton.Patcher.log";

        public static string GetHtmlDiff(string a, string b)
        {
            var dmp = new DiffMatchPatch.diff_match_patch();
            var diffmain = dmp.diff_main(a, b);
            dmp.diff_cleanupSemantic(diffmain);
            return "<div id='hook_diff'><pre>" + dmp.diff_prettyHtml(diffmain) + "</pre></div>";
        }

        public static int Main(string[] args)
        {
            bool interactive = args.Length > 0;

            gendiffs = args.Any(arg => arg == "--generatediffs" || arg == "-gd");

            string logfile = (from arg in args
                              where arg.StartsWith("--logFile:", StringComparison.Ordinal) || arg.StartsWith("-l:", StringComparison.Ordinal)
                              select arg.Split(':').Last()).FirstOrDefault();
            
            LogFile = String.IsNullOrEmpty(logfile) ? logfile : LogFile;

            LogToFile = String.IsNullOrEmpty(logfile);

            if (gendiffs)
                MethodDB.GetInstance();

            newAssCS = true;

            Log($"[( Pluton Patcher v{Version} )]");

            rustAssembly = AssemblyPatcher.GetPatcher("Assembly-CSharp.dll");

            if (rustAssembly.GetType("PlutonPatched") != null) {
                LogError("Assembly-CSharp.dll is already patched!");
                if (interactive)
                    System.Threading.Thread.Sleep(250);
                return (int)ExitCode.ACDLL_ALREADY_PATCHED;
            }

            foreach (var json in Directory.GetFiles("./", "*.json")) {
                JSON.Object jsonObj = JSON.Object.Parse(File.ReadAllText(json));
                var assemblyPatch = AssemblyPatch.ParseFromJSON(jsonObj);
                if (!assemblyPatch.Patch()) {
                    LogError("Failed to patch!");
                    if (interactive)
                        System.Threading.Thread.Sleep(250);
                    return (int)ExitCode.ACDLL_GENERIC_PATCH_ERR;
                }
            }

            if (gendiffs)
            {
                MethodDB.GetInstance().Save();

                string diffs = MethodDB.GetDifferences();

                if (String.IsNullOrEmpty(diffs))
                    File.WriteAllText($"diffs-{DateTime.Now.ToShortDateString()}{DateTime.Now.ToShortTimeString()}.html", "<html><head><style>del,ins{text-decoration:none}ins{background-color:#0F0}del{color:#999;background-color:#F00}</style></head><body>" + diffs + "</body></html>");
            }

            Log("Completed!");

            if (interactive)
                System.Threading.Thread.Sleep(250);

            return (int)ExitCode.SUCCESS;
        }

        public static void Log(string log)
        {
            if (LogToFile) File.AppendAllText(LogFile, log);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(log);
        }

        public static void LogLine(string log)
        {
            if (LogToFile) File.AppendAllText(LogFile, log);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(log);
        }

        public static void LogError(string log)
        {
            if (LogToFile) File.AppendAllText(LogFile, log);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(log);
        }

        public static void LogErrorLine(string log)
        {
            if (LogToFile) File.AppendAllText(LogFile, log);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(log);
        }

        public static void LogException(Exception ex)
        {
            LogErrorLine(ex.ToString());
        }
    }

    public enum ExitCode {
        SUCCESS = 0,
        DLL_MISSING = 10,
        DLL_READ_ERROR = 20,
        DLL_WRITE_ERROR = 21,
        ACDLL_ALREADY_PATCHED = 30,
        ACDLL_GENERIC_PATCH_ERR = 40
    }
}
