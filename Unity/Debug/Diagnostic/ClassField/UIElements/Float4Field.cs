using AnotherECS.Mathematics;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class Float4Field : BaseCompositeField<float4, SFloatField, sfloat>
    {
        public new class UxmlFactory : UxmlFactory<Float4Field, UxmlTraits> { }

        public new class UxmlTraits : BaseField<float4>.UxmlTraits
        {
            private UxmlSfloatAttributeDescription m_XValue = new()
            {
                name = "x"
            };

            private UxmlSfloatAttributeDescription m_YValue = new()
            {
                name = "y"
            };

            private UxmlSfloatAttributeDescription m_ZValue = new()
            {
                name = "z"
            };
            private UxmlSfloatAttributeDescription m_WValue = new()
            {
                name = "w"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                Float4Field vector4Field = (Float4Field)ve;
                vector4Field.SetValueWithoutNotify(new float4(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc), m_ZValue.GetValueFromBag(bag, cc), m_WValue.GetValueFromBag(bag, cc)));
            }
        }

        public new static readonly string ussClassName = "unity-vector4-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        internal override FieldDescription[] DescribeFields()
        {
            return new FieldDescription[]
            {
                new FieldDescription("X", "unity-x-input", (float4 r) => r.x, delegate(ref float4 r, sfloat v)
                {
                    r.x = v;
                }),
                new FieldDescription("Y", "unity-y-input", (float4 r) => r.y, delegate(ref float4 r, sfloat v)
                {
                    r.y = v;
                }),
                new FieldDescription("Z", "unity-z-input", (float4 r) => r.z, delegate(ref float4 r, sfloat v)
                {
                    r.z = v;
                }),
                new FieldDescription("Z", "unity-z-input", (float4 r) => r.w, delegate(ref float4 r, sfloat v)
                {
                    r.w = v;
                }),
            };
        }

        public Float4Field()
            : this(null) { }

        public Float4Field(string label)
            : base(label, 4)
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
        }
    }
}