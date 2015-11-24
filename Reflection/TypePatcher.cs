using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

namespace Pluton.Patcher.Reflection
{
    public class TypePatcher : PatcherObject
    {
        internal TypeDefinition typeDefinition;

        public TypePatcher(PatcherObject prnt, TypeDefinition typDef) : base(prnt)
        {
            typeDefinition = typDef;
            rootAssemblyPatcher = prnt.rootAssemblyPatcher;
        }

        public FieldDefinition GetField(string field)
        {
            return typeDefinition.GetField(field);
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

