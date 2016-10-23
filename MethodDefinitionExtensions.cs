namespace Pluton.Patcher
{
	using System;
	using System.Linq;

	using Mono.Cecil;

	static class MethodDefinitionExtensions
	{
		public static string GetSigniture(this MethodDefinition self)
		{
			return $"{self.Name}({String.Join(",", (from param in self.Parameters select param.ParameterType.Name))})";
		}

		public static MethodDefinition SetPublic(this MethodDefinition self, bool value)
		{
			if (self == null) {
				throw new ArgumentNullException(nameof(self));
			}

			self.IsPublic = value;
			self.IsPrivate = !value;

			return self;
		}

		public static string Print(this MethodDefinition self)
		{
			return self.PrintIL() + Environment.NewLine + self.PrintCSharp();
		}

		public static string PrintIL(this MethodDefinition self)
		{
			try {
				var textoutput = new ICSharpCode.Decompiler.PlainTextOutput();
				var disasm = new ICSharpCode.Decompiler.Disassembler.ReflectionDisassembler(textoutput, true, new System.Threading.CancellationToken());
				disasm.DisassembleMethod(self);
				return textoutput.ToString();
			} catch (Exception ex) {
				MainClass.LogException(ex);
				return ex.ToString();
			}
		}

		public static string PrintCSharp(this MethodDefinition self)
		{
			try {
				var textoutput = new ICSharpCode.Decompiler.PlainTextOutput();
				var builder = new ICSharpCode.Decompiler.Ast.AstBuilder(new ICSharpCode.Decompiler.DecompilerContext(self.DeclaringType.Module) {
					CancellationToken = new System.Threading.CancellationToken(),
					CurrentType = self.DeclaringType,
					Settings = new ICSharpCode.Decompiler.DecompilerSettings()
				});
				builder.AddMethod(self);
				builder.GenerateCode(textoutput);
				return textoutput.ToString();
			} catch (Exception ex) {
				MainClass.LogException(ex);
				return ex.ToString();
			}
		}
	}
}