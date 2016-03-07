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
                var options = new ICSharpCode.ILSpy.DecompilationOptions();
                var lang = new ICSharpCode.ILSpy.ILLanguage(true);
                lang.DecompileMethod(self, textoutput, options);
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
                var options = new ICSharpCode.ILSpy.DecompilationOptions();
                var lang = new ICSharpCode.ILSpy.CSharpLanguage();
                lang.DecompileMethod(self, textoutput, options);
                return textoutput.ToString();
            } catch (Exception ex) {
                MainClass.LogException(ex);
                return ex.ToString();
            }
        }
	}
}