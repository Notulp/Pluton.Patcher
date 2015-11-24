using System;
using System.Collections.Generic;

namespace Pluton.Patcher
{
    public class MethodPatch
    {
        public Reflection.MethodPatcher TargetMethod;

        public List<InstructionPatch> Patches = new List<InstructionPatch>();

        public MethodPatch() {}

        public bool Patch()
        {
            try {
                foreach (var patch in Patches) {
                    switch (patch.InstructionType) {
                    case InstructionType.Clear:
                        TargetMethod.Clear();
                        break;

                    case InstructionType.Append:
                        TargetMethod.Append(patch.Build());
                        break;

                    case InstructionType.InsertAfter:
                        TargetMethod.InsertAfter(patch.InsertOffset, patch.Build());
                        break;

                    case InstructionType.InsertBefore:
                        TargetMethod.InsertBefore(patch.InsertOffset, patch.Build());
                        break;

                    case InstructionType.InsertBeforeRet:
                        TargetMethod.InsertBeforeRet(patch.Build());
                        break;

                    case InstructionType.RemoveAt:
                        TargetMethod.RemoveAt(patch.RemoveStart);
                        break;

                    case InstructionType.RemoveRange:
                        TargetMethod.RemoveRange(patch.RemoveStart, patch.RemoveEnd);
                        break;
                    }
                }
                return true;
            } catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }
        }

        public static MethodPatch ParseFromJSON(JSON.Object obj, Reflection.TypePatcher targetType)
        {
            var patch = new MethodPatch();
            if (obj.ContainsKey("TargetMethodSigniture"))
                patch.TargetMethod = targetType.GetMethod(obj["TargetMethod"].Str, obj["TargetMethodSigniture"].Str);
            else
                patch.TargetMethod = targetType.GetMethod(obj["TargetMethod"].Str);
            Console.WriteLine(" + " + obj["TargetMethod"].Str);
            foreach (JSON.Value instru in obj["Instructions"].Array) {
                var instrupatch = InstructionPatch.ParseFromJSON(instru.Obj, patch);
                switch (instrupatch.OperandType) {
                case OperandType.Instruction:
                    instrupatch.Operand = patch.TargetMethod.IlProc.Body.Instructions[instrupatch.ParamOrVarOffset];
                    break;
                case OperandType.Variable:
                    instrupatch.Operand = patch.TargetMethod.IlProc.Body.Variables[instrupatch.ParamOrVarOffset];
                    break;
                case OperandType.Parameter:
                    instrupatch.Operand = patch.TargetMethod.IlProc.Body.Method.Parameters[instrupatch.ParamOrVarOffset];
                    break;
                }

                patch.Patches.Add(instrupatch);
            }

            return patch;
        }
    }
}

