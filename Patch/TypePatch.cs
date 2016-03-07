namespace Pluton.Patcher
{
    using System;
    using System.Collections.Generic;

    public class TypePatch : BasePatch
    {
        // TODO: Add support to create a Method

        public Reflection.TypePatcher TargetType;

        public List<BasePatch> Patches = new List<BasePatch>();
        public List<TypeInstruction> Instructions = new List<TypeInstruction>();

        override public bool Patch()
        {
            foreach (var patch in Patches) {
                if (!patch.Patch())
                    return false;
            }
            foreach (var instruction in Instructions) {
                if (instruction.InstructionType == TypeInstruction.EInstructionType.SetVisibility)
                    TargetType.Public = instruction.Public;
            }
            return true;
        }

        new internal static BasePatch ParseFromJSON(JSON.Object obj, params object[] args)
        {
            var targettype = args[0] as string;
            var targetAssembly = args[1] as Reflection.AssemblyPatcher;

            var patch = new TypePatch();

            patch.TargetType = targetAssembly.GetType(targettype);

            if (obj.ContainsKey("Instructions")) {
                foreach (JSON.Value instru in obj["Instructions"].Array) {
                    var instruction = TypeInstruction.ParseFromJSON(instru.Obj);

                    if (instruction.InstructionType == TypeInstruction.EInstructionType.CreateMethod)
                        throw new NotImplementedException("Creating method is not yet a supported feature.");

                    if (instruction.InstructionType == TypeInstruction.EInstructionType.CreateField) {
                        patch.TargetType.CreateField(instruction.Name);
                    }

                    patch.Instructions.Add(instruction);
                }
            }

            MainClass.LogLine(targettype);
            if (obj.ContainsKey("Methods")) {
                foreach (JSON.Value met in obj["Methods"].Array) {
                    var methodPatch = MethodPatch.ParseFromJSON(met.Obj, patch.TargetType);
                    patch.Patches.Add(methodPatch);
                }
            }
            if (obj.ContainsKey("Fields")) {
                foreach (JSON.Value fld in obj["Fields"].Array) {
                    var fieldPatch = FieldPatch.ParseFromJSON(fld.Obj, patch.TargetType);
                    patch.Patches.Add(fieldPatch);
                }
            }
            return patch;
        }


    }
}

