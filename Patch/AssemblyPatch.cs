using System;
using System.Collections.Generic;

namespace Pluton.Patcher
{
    public class AssemblyPatch : BasePatch
    {
        public Reflection.AssemblyPatcher TargetAssembly;

        public string FileName;

        public List<BasePatch> Patches = new List<BasePatch>();

        public List<AssemblyInstruction> Instructions = new List<AssemblyInstruction>();

        public AssemblyPatch() {}

        override public bool Patch()
        {
            foreach (var patch in Patches) {
                if (!patch.Patch())
                    return false;
            }
            foreach (var instruction in Instructions) {
                if (instruction.InstructionType == AssemblyInstruction.EInstructionType.WriteToFile) {
                    Console.WriteLine("-> Write assembly to file: " + instruction.Name);
                    TargetAssembly.Write(instruction.Name);
                }
            }
            return true;
        }

        public static AssemblyPatch ParseFromJSON(JSON.Object obj)
        {
            var patch = new AssemblyPatch();

            patch.TargetAssembly = Reflection.AssemblyPatcher.GetPatcher(obj["TargetAssembly"].Str);

            patch.FileName = obj["TargetAssembly"].Str;

            if (obj.ContainsKey("Instructions")) {
                foreach (var inst in obj["Instructions"].Array) {
                    AssemblyInstruction instruction = (AssemblyInstruction)AssemblyInstruction.ParseFromJSON(inst.Obj);

                    if (instruction.InstructionType == AssemblyInstruction.EInstructionType.CreateType) {
                        Console.WriteLine("-> Creating type: " + instruction.Name);
                        patch.TargetAssembly.CreateType(instruction.Name);
                    }

                    patch.Instructions.Add(instruction);
                }
            }

            foreach (var typ in obj["Types"].Obj) {
                var typePatch = TypePatch.ParseFromJSON(typ.Value.Obj, typ.Key, patch.TargetAssembly);
                patch.Patches.Add(typePatch);
            }
            return patch;
        }
    }
}

