using AnotherECS.Mathematics;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class Float3Field : BaseCompositeField<float3, SFloatField, sfloat>
    {
        public new class UxmlFactory : UxmlFactory<Float3Field, UxmlTraits> { }

        public new class UxmlTraits : BaseField<float3>.UxmlTraits
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

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                Float3Field vector3Field = (Float3Field)ve;
                vector3Field.SetValueWithoutNotify(new float3(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc), m_ZValue.GetValueFromBag(bag, cc)));
            }
        }

        public new static readonly string ussClassName = "unity-vector3-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        internal override FieldDescription[] DescribeFields()
        {
            return new FieldDescription[]
            {
                new FieldDescription("X", "unity-x-input", (float3 r) => r.x, delegate(ref float3 r, sfloat v)
                {
                    r.x = v;
                }),
                new FieldDescription("Y", "unity-y-input", (float3 r) => r.y, delegate(ref float3 r, sfloat v)
                {
                    r.y = v;
                }),
                new FieldDescription("Z", "unity-z-input", (float3 r) => r.z, delegate(ref float3 r, sfloat v)
                {
                    r.z = v;
                }),
            };
        }

        public Float3Field()
            : this(null) { }

        public Float3Field(string label)
            : base(label, 3)
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
        }
    }
}