using System;

namespace Pluton.Patcher
{
    public class AssemblyInstruction : BaseInstruction
    {
        public EInstructionType InstructionType;

        // if (InstructionType == EInstructionType.CreateType) Name = NameOfTheType;
        // else if (InstructionType == EInstructionType.WriteToFile) Name = FileName;
        public string Name;

        new internal static BaseInstruction ParseFromJSON(JSON.Object obj, params object[] nothing)
        {
            var instruction = new AssemblyInstruction();

            instruction.InstructionType = (EInstructionType)Enum.Parse(typeof(EInstructionType), obj["InstructionType"].Str, true);
            instruction.Name = obj["Name"].Str;

            return instruction;
        }

        public enum EInstructionType
        {
            CreateType,
            WriteToFile
        }
    }
}

