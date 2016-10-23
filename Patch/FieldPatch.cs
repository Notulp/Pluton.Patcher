namespace Pluton.Patcher
{
	using System.Collections.Generic;

	public class FieldPatch : BasePatch
	{
		public Reflection.FieldPatcher TargetField;

		public List<FieldInstruction> Instructions = new List<FieldInstruction>();

		override public bool Patch()
		{
			foreach (var patch in Instructions) {
				if (patch.Constant != null) {
					TargetField.SetConstant(patch.Value);
				}
				if (patch.Public != null) {
					TargetField.SetPublic(patch.Public == true);
				}
				if (patch.Static != null) {
					TargetField.SetStatic(patch.Static == true);
				}
				if (patch.ReadOnly != null) {
					TargetField.SetReadOnly(patch.ReadOnly == true);
				}
			}
			return true;
		}

		new internal static FieldPatch ParseFromJSON(JSON.Object obj, params object[] args)
		{
			var targetType = args[0] as Reflection.TypePatcher;

			var patch = new FieldPatch();

			patch.TargetField = targetType.GetField(obj["TargetField"].Str);

			foreach (JSON.Value instru in obj["Instructions"].Array) {
				var instruction = FieldInstruction.ParseFromJSON(instru.Obj);
				patch.Instructions.Add(instruction);
			}

			return patch;
		}
	}
}

