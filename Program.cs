using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Pluton.Patcher.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Pluton.Patcher
{
    class MainClass
    {
        public static AssemblyPatcher rustAssembly;
        public static string Version { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        internal static bool gendiffs = false;
        internal static bool newAssCS = false;

        public static string GetHtmlDiff(string a, string b)
        {
            var dmp = new DiffMatchPatch.diff_match_patch();
            var diffmain = dmp.diff_main(a, b);
            dmp.diff_cleanupSemantic(diffmain);
            return "<div id='hook_diff'><pre>" + dmp.diff_prettyHtml(diffmain) + "</pre></div>";
        }

        public static int Main(string[] args)
        {
            bool interactive = true;
            if (args.Length > 0)
                interactive = false;

            // TODO: new way doesn't generate differences yet, so add it
            foreach (string arg in args)
                if (arg.Contains("--generatediffs"))
                    gendiffs = true;

            newAssCS = true;

            Console.WriteLine(string.Format("[( Pluton Patcher v{0} )]", Version));

            rustAssembly = AssemblyPatcher.GetPatcher("Assembly-CSharp.dll");

            if (rustAssembly.GetType("PlutonPatched") != null) {
                Console.WriteLine("Assembly-CSharp.dll is already patched!");
                return (int)ExitCode.ACDLL_ALREADY_PATCHED;
            }

            foreach (var json in Directory.GetFiles("./", "*.json")) {
                JSON.Object jsonObj = JSON.Object.Parse(File.ReadAllText(json));
                var assemblyPatch = AssemblyPatch.ParseFromJSON(jsonObj);
                if (!assemblyPatch.Patch()) {
                    Console.WriteLine("Failed to patch!");
                    return (int)ExitCode.ACDLL_GENERIC_PATCH_ERR;
                }
            }
            //Check if patching is required
            // TODO: add support for adding new Types/Fields/Methods via json
            /*TypePatcher plutonClass = rustAssembly.GetType("PlutonPatched");
            if (plutonClass == null) {
                try {
                    if (gendiffs) {
                        string hash = String.Empty;
                        using (var sha512 = new System.Security.Cryptography.SHA512Managed())
                            hash = BitConverter.ToString(sha512.ComputeHash(File.ReadAllBytes("Assembly-CSharp.dll"))).Replace("-", "").ToLower();

                        Directory.CreateDirectory("diffs");

                        string hashpath = "diffs" + Path.DirectorySeparatorChar + "lastHash";

                        if (File.Exists(hashpath)) newAssCS = hash != File.ReadAllText(hashpath);

                        if (newAssCS) {
                            foreach (var difffile in Directory.GetFiles("diffs")) {
                                if (difffile.Contains(".htm")) {
                                    string filename = Path.GetFileName(difffile);
                                    string dirname = Path.GetDirectoryName(difffile);
                                    Directory.CreateDirectory(Path.Combine(dirname, "old"));
                                    File.Move(difffile, difffile.Replace(Path.Combine(dirname, filename), Path.Combine(dirname, "old", filename)));
                                }
                            }
                        }

                        if (gendiffs && newAssCS)
                            File.WriteAllText(hashpath, hash);
                    }

                    Console.WriteLine("Patched Assembly-CSharp !");
                } catch (Exception ex) {
                    interactive = true;
                    Console.WriteLine("An error occured while patching Assembly-CSharp :");
                    Console.WriteLine();
                    Console.WriteLine(ex.Message.ToString());

                    //Normal handle for the others
                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace.ToString());
                    Console.WriteLine();

                    if (interactive) {
                        Console.WriteLine("Press any key to continue...");
                    }
                    return (int)ExitCode.ACDLL_GENERIC_PATCH_ERR;
                }
            } else {
                Console.WriteLine("Assembly-CSharp.dll is already patched!");
                return (int)ExitCode.ACDLL_ALREADY_PATCHED;
            }*/

            //Successfully patched the server
            Console.WriteLine("Completed !");

            if (interactive)
                System.Threading.Thread.Sleep(250);

            return (int)ExitCode.SUCCESS;
        }
    }

    public enum ExitCode : int {
        SUCCESS = 0,
        DLL_MISSING = 10,
        DLL_READ_ERROR = 20,
        DLL_WRITE_ERROR = 21,
        ACDLL_ALREADY_PATCHED = 30,
        ACDLL_GENERIC_PATCH_ERR = 40
    }
}
