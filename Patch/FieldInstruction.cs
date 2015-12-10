using System;

namespace Pluton.Patcher
{
    public class FieldInstruction : BaseInstruction
    {
        // TODO: Add support for static fields and constructors
        // more predefined or remove the only one as iys just a hacky way to do it

        public EInstructionType InstructionType;
        public EValueSource ValueSource;

        public bool? Public;

        public bool? Static;

        public bool? ReadOnly;

        public bool? Constant;

        public object Value;

        public FieldInstruction() { }

        public static FieldInstruction ParseFromJSON(JSON.Object obj)
        {
            var instruction = new FieldInstruction();

            instruction.InstructionType = (EInstructionType)Enum.Parse(typeof(EInstructionType), obj["InstructionType"].Str, true);
            if (obj.ContainsKey("ValueSource")) {
                instruction.ValueSource = (EValueSource)Enum.Parse(typeof(EValueSource), obj["ValueSource"].Str, true);

                switch (instruction.ValueSource) {
                case EValueSource.StaticField:
                case EValueSource.TypeConstruction:
                    throw new NotImplementedException(instruction.ValueSource.ToString() + " as ValueSource is not yet implemented.");

                case EValueSource.Custom:
                    instruction.Value = obj.ContainsKey("Value") ? obj["Value"].value : null;
                    break;

                case EValueSource.PreDefined:
                    if (obj["Value"].Str == "%PatcherVersion%") {
                        instruction.Value = Pluton.Patcher.MainClass.Version;
                    }
                    break;
                }
            }
            instruction.Public = obj.ContainsKey("Public") ? obj["Public"].Boolean : instruction.Public;
            instruction.Static = obj.ContainsKey("Static") ? obj["Static"].Boolean : instruction.Static;
            instruction.ReadOnly = obj.ContainsKey("ReadOnly") ? obj["ReadOnly"].Boolean : instruction.ReadOnly;
            instruction.Constant = obj.ContainsKey("Constant") ? obj["Constant"].Boolean : instruction.Constant;

            return instruction;
        }

        public enum EInstructionType
        {
            SetValue,
            SetVisibility
        }

        public enum EValueSource
        {
            StaticField,
            TypeConstruction,
            Custom,
            PreDefined
        }
    }
}

