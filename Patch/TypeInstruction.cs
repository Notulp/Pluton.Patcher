using System;

namespace Pluton.Patcher
{
    public class TypeInstruction : BaseInstruction
    {
        // TODO: 1. add support for creating nested types
        //          tho it's probably possible from AssemblyInstruction,
        //          by setting the name like: TypeToCreateNestedTypeIn/NestedType'sName
        //       2. add support to set the type of the field and return type of the method
        //          fields will be typeof(object) for now, method's are not supported yet, so throw exception

        public EInstructionType InstructionType;

        public bool Public = true;

        // the name of the field/method to be created
        public string Name;

        public TypeInstruction() {}

        public static TypeInstruction ParseFromJSON(JSON.Object obj)
        {
            var instruction = new TypeInstruction();

            instruction.InstructionType = (EInstructionType)Enum.Parse(typeof(EInstructionType), obj["InstructionType"].Str, true);

            instruction.Name = obj.ContainsKey("Name") ? obj["Name"].Str : null;

            instruction.Public = obj.ContainsKey("Public") ? obj["Public"].Boolean : instruction.Public;

            return instruction;
        }

        public enum EInstructionType
        {
            CreateMethod,
            CreateField,
            SetVisibility
        }
    }
}

