using AnotherECS.Mathematics;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class Int3Field : BaseCompositeField<int3, IntegerField, int>
    {
        public new class UxmlFactory : UxmlFactory<Int3Field, UxmlTraits> { }

        public new class UxmlTraits : BaseField<int3>.UxmlTraits
        {
            private UxmlIntAttributeDescription m_XValue = new()
            {
                name = "x"
            };

            private UxmlIntAttributeDescription m_YValue = new()
            {
                name = "y"
            };

            private UxmlIntAttributeDescription m_ZValue = new()
            {
                name = "z"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                Int3Field vector3Field = (Int3Field)ve;
                vector3Field.SetValueWithoutNotify(new int3(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc), m_ZValue.GetValueFromBag(bag, cc)));
            }
        }

        public new static readonly string ussClassName = "unity-vector3-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        internal override FieldDescription[] DescribeFields()
        {
            return new FieldDescription[]
            {
                new FieldDescription("X", "unity-x-input", (int3 r) => r.x, delegate(ref int3 r, int v)
                {
                    r.x = v;
                }),
                new FieldDescription("Y", "unity-y-input", (int3 r) => r.y, delegate(ref int3 r, int v)
                {
                    r.y = v;
                }),
                new FieldDescription("Z", "unity-z-input", (int3 r) => r.y, delegate(ref int3 r, int v)
                {
                    r.z = v;
                }),
            };
        }

        public Int3Field()
            : this(null) { }

        public Int3Field(string label)
            : base(label, 3)
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
        }
    }
}