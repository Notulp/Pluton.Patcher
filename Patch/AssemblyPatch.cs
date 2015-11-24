using System;
using System.Collections.Generic;

namespace Pluton.Patcher
{
    public class AssemblyPatch
    {
        // Add support to create a Type

        public Reflection.AssemblyPatcher TargetAssembly;

        public string FileName;

        public List<TypePatch> Patches = new List<TypePatch>();

        public AssemblyPatch() {}

        public bool Patch()
        {
            foreach (var patch in Patches) {
                if (!patch.Patch())
                    return false;
            }
            TargetAssembly.Write(FileName);
            return true;
        }

        public static AssemblyPatch ParseFromJSON(JSON.Object obj)
        {
            var patch = new AssemblyPatch();

            patch.TargetAssembly = Reflection.AssemblyPatcher.GetPatcher(obj["TargetAssembly"].Str);

            patch.FileName = obj["TargetAssembly"].Str;

            foreach (var typ in obj["Types"].Obj) {
                var typePatch = TypePatch.ParseFromJSON(typ.Key, typ.Value.Obj, patch.TargetAssembly);
                patch.Patches.Add(typePatch);
            }
            return patch;
        }
    }
}

