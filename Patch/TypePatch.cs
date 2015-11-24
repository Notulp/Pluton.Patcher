using System;
using System.Collections.Generic;

namespace Pluton.Patcher
{
    public class TypePatch
    {
        // TODO: Add support to create a Field/Method

        public Reflection.TypePatcher TargetType;

        public List<MethodPatch> Patches = new List<MethodPatch>();

        public TypePatch() {}

        public bool Patch()
        {
            foreach (var patch in Patches) {
                if (!patch.Patch())
                    return false;
            }
            return true;
        }

        public static TypePatch ParseFromJSON(string targettype, JSON.Object obj, Reflection.AssemblyPatcher targetAssembly)
        {
            var patch = new TypePatch();

            patch.TargetType = targetAssembly.GetType(targettype);

            Console.WriteLine(targettype);
            foreach (JSON.Value met in obj["Methods"].Array) {
                var methodPatch = MethodPatch.ParseFromJSON(met.Obj, patch.TargetType);
                patch.Patches.Add(methodPatch);
            }
            return patch;
        }


    }
}

