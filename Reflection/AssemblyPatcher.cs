namespace Pluton.Patcher.Reflection {
    using System;
    using Mono.Cecil;
    using System.Linq;
    using System.Collections.Generic;

    public class AssemblyPatcher : PatcherObject {
        internal AssemblyDefinition assemblyDefinition;
        internal ModuleDefinition mainModule;

        public static Dictionary<string, AssemblyPatcher> AssemblyCache = new Dictionary<string, AssemblyPatcher>();

        internal Mono.Collections.Generic.Collection<TypeDefinition> Types => mainModule.Types;

        public AssemblyPatcher(AssemblyDefinition assDef) {
            if (assDef == null)
                return;
            
            assemblyDefinition = assDef;
            mainModule = assemblyDefinition.MainModule;
            rootAssemblyPatcher = this;
        }

        public static AssemblyPatcher GetPatcher(string assName) {
            AssemblyPatcher result;
            if (AssemblyCache.ContainsKey(assName)) {
                result = AssemblyCache[assName];
            } else {
                result = AssemblyPatcher.FromFile(assName);
                AssemblyCache.Add(assName, result);
            }
            return result;
        }

        internal static AssemblyPatcher FromFile(string filename) => new AssemblyPatcher(AssemblyDefinition.ReadAssembly(filename));

        public TypePatcher CreateType(string fullname) => CreateType(fullname, TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit, typeof(Object));

        public TypePatcher CreateType(string fullname, TypeAttributes attribs, Type Base) {
            var nameSpace = "";
            var name = fullname;
            if (fullname.Contains('.')) {
                nameSpace = fullname.Remove(fullname.LastIndexOf('.'));
                name = name.Remove(0, name.LastIndexOf('.'));
            }
            return CreateType(nameSpace, name, attribs, Base);
        }

        public TypePatcher CreateType<T>(string fullname, TypeAttributes attribs) => CreateType(fullname, attribs, typeof(T));

        public TypePatcher CreateType(string nameSpace, string name) => CreateType(nameSpace + "." + name);

        public TypePatcher CreateType(string nameSpace, string name, TypeAttributes attribs, Type Base) {
            TypeDefinition newClass = new TypeDefinition(nameSpace, name, attribs, mainModule.Import(Base));
            mainModule.Types.Add(newClass);
            mainModule.Import(newClass);

            if (nameSpace == "")
                return GetType(name);
            return GetType(nameSpace + "." + name);
        }

        public TypePatcher CreateType<T>(string nameSpace, string name, TypeAttributes attribs) => CreateType(nameSpace, name, attribs, typeof(T));

        public TypePatcher GetType(string type) {
            var t = mainModule.GetType(type);
            if (t == null) return null;

            return new TypePatcher(this, t);
        }

        public TypePatcher GetType(Func<IEnumerable<TypeDefinition>, TypeDefinition> func) => new TypePatcher(this, func.Invoke(mainModule.GetTypes()));

        public MethodReference ImportMethod(MethodPatcher toImport) => mainModule.Import(toImport.methodDefinition);

        public void Write(string file) => assemblyDefinition.Write(file);
    }
}

