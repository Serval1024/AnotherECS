using AnotherECS.Mathematics;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class Float2Field : BaseCompositeField<float2, SFloatField, sfloat>
    {
        public new class UxmlFactory : UxmlFactory<Float2Field, UxmlTraits> { }

        public new class UxmlTraits : BaseField<float2>.UxmlTraits
        {
            private UxmlSfloatAttributeDescription m_XValue = new()
            {
                name = "x"
            };

            private UxmlSfloatAttributeDescription m_YValue = new()
            {
                name = "y"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                Float2Field vector2Field = (Float2Field)ve;
                vector2Field.SetValueWithoutNotify(new float2(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc)));
            }
        }

        public new static readonly string ussClassName = "unity-vector2-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        internal override FieldDescription[] DescribeFields()
        {
            return new FieldDescription[]
            {
                new FieldDescription("X", "unity-x-input", (float2 r) => r.x, delegate(ref float2 r, sfloat v)
                {
                    r.x = v;
                }),
                new FieldDescription("Y", "unity-y-input", (float2 r) => r.y, delegate(ref float2 r, sfloat v)
                {
                    r.y = v;
                }),
            };
        }

        public Float2Field()
            : this(null) { }

        public Float2Field(string label)
            : base(label, 2)
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
        }
    }
}