using System;
using System.Collections.Generic;

namespace Pluton.Patcher
{
    public class MethodPatch : BasePatch
    {
        public Reflection.MethodPatcher TargetMethod;

        public List<MethodInstruction> Instructions = new List<MethodInstruction>();

        public MethodPatch() {}

        override public bool Patch()
        {
            try {
                foreach (var patch in Instructions) {
                    switch (patch.InstructionType) {
                    case EInstructionType.Clear:
                        TargetMethod.Clear();
                        break;

                    case EInstructionType.Append:
                        TargetMethod.Append(patch.Build());
                        break;

                    case EInstructionType.InsertAfter:
                        TargetMethod.InsertAfter(patch.InsertOffset, patch.Build());
                        break;

                    case EInstructionType.InsertBefore:
                        TargetMethod.InsertBefore(patch.InsertOffset, patch.Build());
                        break;

                    case EInstructionType.InsertBeforeRet:
                        TargetMethod.InsertBeforeRet(patch.Build());
                        break;

                    case EInstructionType.RemoveAt:
                        TargetMethod.RemoveAt(patch.RemoveStart);
                        break;

                    case EInstructionType.RemoveRange:
                        TargetMethod.RemoveRange(patch.RemoveStart, patch.RemoveEnd);
                        break;

                    case EInstructionType.SetVisibility:
                        TargetMethod.Public = patch.Public;
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

        new internal static BasePatch ParseFromJSON(JSON.Object obj, params object[] args)
        {
            Reflection.TypePatcher targetType = args[0] as Reflection.TypePatcher;

            var patch = new MethodPatch();

            if (obj.ContainsKey("TargetMethodSigniture"))
                patch.TargetMethod = targetType.GetMethod(obj["TargetMethod"].Str, obj["TargetMethodSigniture"].Str);
            else
                patch.TargetMethod = targetType.GetMethod(obj["TargetMethod"].Str);
            
            Console.WriteLine(" + " + patch.TargetMethod.methodDefinition.GetSigniture());

            foreach (JSON.Value instru in obj["Instructions"].Array) {
                var instrupatch = MethodInstruction.ParseFromJSON(instru.Obj, patch);

                switch (instrupatch.OperandType) {
                case EOperandType.Instruction:
                    instrupatch.Operand = patch.TargetMethod.IlProc.Body.Instructions[instrupatch.ParamOrVarOffset];
                    break;
                case EOperandType.Variable:
                    instrupatch.Operand = patch.TargetMethod.IlProc.Body.Variables[instrupatch.ParamOrVarOffset];
                    break;
                case EOperandType.Parameter:
                    instrupatch.Operand = patch.TargetMethod.IlProc.Body.Method.Parameters[instrupatch.ParamOrVarOffset];
                    break;
                }

                patch.Instructions.Add(instrupatch);
            }

            return patch;
        }
    }
}

