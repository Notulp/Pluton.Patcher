namespace Pluton.Patcher {
	using System;
	using Mono.Cecil.Cil;

	public class MethodInstruction : BaseInstruction {
		// TODO: Add support for adding try-catch-finally, switch? (should be possible already, but could make it easier), using statments

		Instruction Instruction;

		public OpCode OpCode;

		public object Operand;

		public EInstructionType InstructionType;

		public EOperandType OperandType = EOperandType.None;

		public int ParamOrVarOffset = -1;

		public int RemoveStart = -1,
			RemoveEnd = -1,
			InsertOffset = -1;

		public bool Public = true;

		public static MethodInstruction ParseFromJSON(JSON.Object instru, MethodPatch targetMethod) {
			var patch = new MethodInstruction();

			patch.InstructionType = (EInstructionType)Enum.Parse(typeof(EInstructionType),
			                                                     instru["InstructionType"].Str);

			switch (patch.InstructionType) {
				case EInstructionType.SetVisibility:
					patch.Public = instru.ContainsKey("Public") ? instru["Public"].Boolean : patch.Public;
					break;

				case EInstructionType.RemoveRange:
					patch.RemoveEnd = Int32.Parse(instru["RemoveEnd"].Str);
					goto case EInstructionType.RemoveAt;

				case EInstructionType.RemoveAt:
					patch.RemoveStart = Int32.Parse(instru["RemoveStart"].Str);
					goto case EInstructionType.Clear;

				case EInstructionType.Clear:
					return patch;

				case EInstructionType.InsertAfter:
				case EInstructionType.InsertBefore:
					patch.InsertOffset = Int32.Parse(instru["InsertOffset"].Str);
					break;
			}

			patch.Instruction = Instruction.Create(OpCodes.Ret);

			patch.OpCode = (OpCode)typeof(OpCodes).GetField(instru["OpCode"].Str).GetValue(typeof(OpCodes));

			patch.OperandType = (EOperandType)Enum.Parse(typeof(EOperandType), instru["OperandType"].Str);

			switch (patch.OperandType) {
				case EOperandType.Type:
					patch.Operand = Reflection.AssemblyPatcher.GetPatcher(instru["TargetAssembly"].Str).GetType(instru["TargetType"].Str).typeDefinition;
					break;

				case EOperandType.Method:
					if (instru.ContainsKey("TargetMethodSigniture"))
						patch.Operand = targetMethod.TargetMethod.rootAssemblyPatcher.mainModule.Import(Reflection.AssemblyPatcher.GetPatcher(instru["TargetAssembly"].Str).GetType(instru["TargetType"].Str).GetMethod(instru["TargetMethod"].Str,
						                                                                                                                                                                                                instru["TargetMethodSigniture"].Str).methodDefinition);
					else
						patch.Operand = targetMethod.TargetMethod.rootAssemblyPatcher.mainModule.Import(Reflection.AssemblyPatcher.GetPatcher(instru["TargetAssembly"].Str).GetType(instru["TargetType"].Str).GetMethod(instru["TargetMethod"].Str).methodDefinition);
					break;

				case EOperandType.Field:
					patch.Operand = Reflection.AssemblyPatcher.GetPatcher(instru["TargetAssembly"].Str).GetType(instru["TargetType"].Str).GetField(instru["TargetField"].Str);
					break;

				case EOperandType.Parameter:
				case EOperandType.Variable:
				case EOperandType.Instruction:
					patch.ParamOrVarOffset = Int32.Parse(instru["ParamVarOffset"].Str);
					break;

				case EOperandType.SByte:
					patch.Operand = SByte.Parse(instru["Operand"].Str);
					break;

				case EOperandType.Byte:
					patch.Operand = Byte.Parse(instru["Operand"].Str);
					break;

				case EOperandType.Int:
					patch.Operand = Int32.Parse(instru["Operand"].Str);
					break;

				case EOperandType.Float:
					patch.Operand = Single.Parse(instru["Operand"].Str);
					break;

				case EOperandType.Long:
					patch.Operand = Int64.Parse(instru["Operand"].Str);
					break;

				case EOperandType.Double:
					patch.Operand = Double.Parse(instru["Operand"].Str);
					break;

				case EOperandType.String:
					patch.Operand = instru["Operand"].Str;
					break;
			}

			return patch;
		}

		public Instruction Build() {
			Instruction.OpCode = OpCode;
			Instruction.Operand = Operand;
			return Instruction;
		}
	}

	public enum EOperandType {
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

	public enum EInstructionType {
		Append,
		Clear,
		InsertAfter,
		InsertBefore,
		InsertBeforeRet,
		RemoveAt,
		RemoveRange,
		SetVisibility
	}
}

