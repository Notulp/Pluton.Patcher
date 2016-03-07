namespace Pluton.Patcher.Reflection
{
    using System;
    using Mono.Cecil;
    using System.Linq;
    using System.Collections.Generic;

    public class TypePatcher : PatcherObject
    {
        internal TypeDefinition typeDefinition;

        public bool Public {
            get {
                return typeDefinition.IsPublic;
            }
            set {
                typeDefinition.IsPublic = value;
                typeDefinition.IsNotPublic = !value;
            }
        }

        public TypePatcher(PatcherObject prnt, TypeDefinition typDef) : base(prnt)
        {
            typeDefinition = typDef;
            rootAssemblyPatcher = prnt.rootAssemblyPatcher;
        }

        public FieldPatcher CreateField(string fieldName)
        {
            return CreateField(fieldName, typeof(object));
        }

        public FieldPatcher CreateField(string fieldname, Type fieldType)
        {
            var field = new FieldDefinition(fieldname, FieldAttributes.CompilerControlled | FieldAttributes.Public, rootAssemblyPatcher.mainModule.Import(fieldType));
            typeDefinition.Fields.Add(field);
            return GetField(fieldname);
        }

        public FieldPatcher GetField(string field)
        {
            return new FieldPatcher(this, typeDefinition.GetField(field));
        }

        public MethodPatcher GetMethod(string method)
        {
            return new MethodPatcher(this, typeDefinition.GetMethod(method));
        }

        public MethodPatcher GetMethod(string method, string sign)
        {
            return GetMethod(methods => {
                return (from   m in methods
                        where  m.Name == method &&
                               m.GetSigniture() == sign
                        select m).FirstOrDefault();
            });
        }

        public MethodPatcher GetMethod(Func<IEnumerable<MethodDefinition>, MethodDefinition> func)
        {
            return new MethodPatcher(this, func.Invoke(typeDefinition.GetMethods()));
        }

        public TypePatcher GetNestedType(string nestedType)
        {
            return new TypePatcher(this, typeDefinition.GetNestedType(nestedType));
        }
    }
}

