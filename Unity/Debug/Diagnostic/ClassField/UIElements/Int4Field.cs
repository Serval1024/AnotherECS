using AnotherECS.Mathematics;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class Int4Field : BaseCompositeField<int4, IntegerField, int>
    {
        public new class UxmlFactory : UxmlFactory<Int4Field, UxmlTraits> { }

        public new class UxmlTraits : BaseField<int4>.UxmlTraits
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

            private UxmlIntAttributeDescription m_WValue = new()
            {
                name = "w"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                Int4Field vector4Field = (Int4Field)ve;
                vector4Field.SetValueWithoutNotify(new int4(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc), m_ZValue.GetValueFromBag(bag, cc), m_WValue.GetValueFromBag(bag, cc)));
            }
        }

        public new static readonly string ussClassName = "unity-vector4-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        internal override FieldDescription[] DescribeFields()
        {
            return new FieldDescription[]
            {
                new FieldDescription("X", "unity-x-input", (int4 r) => r.x, delegate(ref int4 r, int v)
                {
                    r.x = v;
                }),
                new FieldDescription("Y", "unity-y-input", (int4 r) => r.y, delegate(ref int4 r, int v)
                {
                    r.y = v;
                }),
                new FieldDescription("Z", "unity-z-input", (int4 r) => r.y, delegate(ref int4 r, int v)
                {
                    r.z = v;
                }),
                new FieldDescription("W", "unity-w-input", (int4 r) => r.y, delegate(ref int4 r, int v)
                {
                    r.w = v;
                }),
            };
        }

        public Int4Field()
            : this(null) { }

        public Int4Field(string label)
            : base(label, 4)
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
        }
    }
}