using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace Pluton.Patcher
{
    public class InstructionPatch
    {
        Instruction Instruction;

        MethodPatch TargetMethod;

        public string TargetAssembly = "Pluton.dll", TargetMethod2, TargetType = "Pluton.Hooks", TargetMethodSigniture;

        public OpCode OpCode;

        public object Operand;

        public InstructionType InstructionType;

        public OperandType OperandType = OperandType.None;

        public int ParamOrVarOffset = -1;

        public int RemoveStart = -1, RemoveEnd = -1, InsertOffset = -1;

        public static InstructionPatch ParseFromJSON(JSON.Object instru, MethodPatch targetMethod)
        {
            var patch = new InstructionPatch();

            patch.TargetMethod = targetMethod;

            patch.Instruction = Instruction.Create(OpCodes.Ret);

            patch.OpCode = (OpCode)typeof(OpCodes).GetField(instru["OpCode"].Str).GetValue(typeof(OpCodes));

            patch.InstructionType = (InstructionType)Enum.Parse(typeof(InstructionType), instru["InstructionType"].Str);

            switch (patch.InstructionType) {
            case InstructionType.RemoveRange:
                patch.RemoveEnd = Int32.Parse(instru["RemoveEnd"].Str);
                goto case InstructionType.RemoveAt;

            case InstructionType.RemoveAt:
                patch.RemoveStart = Int32.Parse(instru["RemoveStart"].Str);
                goto case InstructionType.Clear;

            case InstructionType.Clear:
                return patch;

            case InstructionType.InsertAfter:
            case InstructionType.InsertBefore:
                patch.InsertOffset = Int32.Parse(instru["InsertOffset"].Str);
                break;
            }

            patch.OperandType = (OperandType)Enum.Parse(typeof(OperandType), instru["OperandType"].Str);

            switch (patch.OperandType) {
            case OperandType.Type:
                patch.Operand = Reflection.AssemblyPatcher.GetPatcher(instru["TargetAssembly"].Str).GetType(instru["TargetType"].Str).typeDefinition;
                break;

            case OperandType.Method:
                if (instru.ContainsKey("TargetMethodSigniture"))
                    patch.Operand = targetMethod.TargetMethod.rootAssemblyPatcher.mainModule.Import(Reflection.AssemblyPatcher.GetPatcher(instru["TargetAssembly"].Str).GetType(instru["TargetType"].Str).GetMethod(instru["TargetMethod"].Str, instru["TargetMethodSigniture"].Str).methodDefinition);
                else
                    patch.Operand = targetMethod.TargetMethod.rootAssemblyPatcher.mainModule.Import(Reflection.AssemblyPatcher.GetPatcher(instru["TargetAssembly"].Str).GetType(instru["TargetType"].Str).GetMethod(instru["TargetMethod"].Str).methodDefinition);
                break;

            case OperandType.Field:
                patch.Operand = Reflection.AssemblyPatcher.GetPatcher(instru["TargetAssembly"].Str).GetType(instru["TargetType"].Str).GetField(instru["TargetField"].Str);
                break;

            case OperandType.Parameter:
            case OperandType.Variable:
            case OperandType.Instruction:
                patch.ParamOrVarOffset = Int32.Parse(instru["ParamVarOffset"].Str);
                break;

            case OperandType.SByte:
                patch.Operand = SByte.Parse(instru["Operand"].Str);
                break;

            case OperandType.Byte:
                patch.Operand = Byte.Parse(instru["Operand"].Str);
                break;

            case OperandType.Int:
                patch.Operand = Int32.Parse(instru["Operand"].Str);
                break;

            case OperandType.Float:
                patch.Operand = Single.Parse(instru["Operand"].Str);
                break;

            case OperandType.Long:
                patch.Operand = Int64.Parse(instru["Operand"].Str);
                break;

            case OperandType.Double:
                patch.Operand = Double.Parse(instru["Operand"].Str);
                break;

            case OperandType.String:
                patch.Operand = instru["Operand"].Str;
                break;

            case OperandType.None:
            default:
                break;
            }

            return patch;
        }

        public JSON.Object ToJSON()
        {
            var result = new JSON.Object();
            if (OpCode != null) {
                string underscored = OpCode.Name.Replace('.', '_');
                string camelCase = System.Text.RegularExpressions.Regex.Replace(underscored, @"(_[a-z]{1})", match => match.ToString().ToUpperInvariant());
                result["OpCode"] = Char.ToUpper(camelCase[0]) + camelCase.Substring(1);
            }
            if (Operand != null)
                result["Operand"] = Operand.ToString();

            result["InstructionType"] = InstructionType.ToString();
            result["OperandType"] = OperandType.ToString();

            if (ParamOrVarOffset != -1)
                result["ParamVarOffset"] = ParamOrVarOffset.ToString();

            if (RemoveEnd != -1)
                result["RemoveEnd"] = RemoveEnd.ToString();

            if (RemoveStart != -1)
                result["RemoveStart"] = RemoveStart.ToString();

            if (InsertOffset != -1)
                result["InsertOffset"] = InsertOffset.ToString();

            if (OperandType == OperandType.Method || OperandType ==  OperandType.Type || OperandType == OperandType.Field) {
                if (TargetMethod2 != null)
                    result["TargetMethod"] = TargetMethod2;

                if (TargetMethodSigniture != null)
                    result["TargetMethodSigniture"] = TargetMethodSigniture;

                result["TargetType"] = TargetType;

                result["TargetAssembly"] = TargetAssembly;
            }
            return result;
        }

        public Instruction Build()
        {
            Instruction.OpCode = OpCode;
            Instruction.Operand = Operand;
            return Instruction;

        }
    }

    public enum OperandType
    {
        None,

        Instruction,

        Type,
        Method,
        Field,
        Parameter,
        Variable,

        SByte,
        Byte,
        Int,
        Float,
        Long,
        Double,
        String
    }

    public enum InstructionType
    {
        Append,
        Clear,
        InsertAfter,
        InsertBefore,
        InsertBeforeRet,
        RemoveAt,
        RemoveRange
    }
}

