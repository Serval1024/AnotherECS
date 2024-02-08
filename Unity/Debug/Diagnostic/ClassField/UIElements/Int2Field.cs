using AnotherECS.Mathematics;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class Int2Field : BaseCompositeField<int2, IntegerField, int>
    {
        public new class UxmlFactory : UxmlFactory<Int2Field, UxmlTraits> { }

        public new class UxmlTraits : BaseField<int2>.UxmlTraits
        {
            private UxmlIntAttributeDescription m_XValue = new()
            {
                name = "x"
            };

            private UxmlIntAttributeDescription m_YValue = new()
            {
                name = "y"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                Int2Field vector2Field = (Int2Field)ve;
                vector2Field.SetValueWithoutNotify(new int2(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc)));
            }
        }

        public new static readonly string ussClassName = "unity-vector2-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        internal override FieldDescription[] DescribeFields()
        {
            return new FieldDescription[]
            {
                new FieldDescription("X", "unity-x-input", (int2 r) => r.x, delegate(ref int2 r, int v)
                {
                    r.x = v;
                }),
                new FieldDescription("Y", "unity-y-input", (int2 r) => r.y, delegate(ref int2 r, int v)
                {
                    r.y = v;
                }),
            };
        }

        public Int2Field()
            : this(null) { }

        public Int2Field(string label)
            : base(label, 2)
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
        }
    }
}