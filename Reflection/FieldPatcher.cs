namespace Pluton.Patcher.Reflection
{
    using Mono.Cecil;

    public class FieldPatcher : PatcherObject
    {
        internal FieldDefinition fieldDefinition;

        public bool Public
        {
            get
            {
                return fieldDefinition.IsPublic;
            }
            set
            {
                fieldDefinition.IsPublic = value;
                fieldDefinition.IsPrivate = !value;
            }
        }

        public bool Static {
            get
            {
                return fieldDefinition.IsStatic;
            }
            set
            {
                fieldDefinition.IsStatic = value;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return fieldDefinition.IsInitOnly;
            }
            set
            {
                fieldDefinition.IsInitOnly = value;
            }
        }

        public FieldPatcher(PatcherObject prnt, FieldDefinition fieldDef) : base(prnt)
        {
            fieldDefinition = fieldDef;
        }

        public void SetPublic(bool value = true) => Public = value;

        public void SetStatic(bool value = true) => Static = value;

        public void SetReadOnly(bool value = true) => ReadOnly = value;

        public void SetConstant(object value = null)
        {
            fieldDefinition.HasConstant = true;
            fieldDefinition.Constant = value;
        }
    }
}

