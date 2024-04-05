using AnotherECS.Mathematics;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class QuaternionField : BaseCompositeField<quaternion, SFloatField, sfloat>
    {
        public new class UxmlFactory : UxmlFactory<QuaternionField, UxmlTraits> { }

        public new class UxmlTraits : BaseField<quaternion>.UxmlTraits
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
                QuaternionField vector4Field = (QuaternionField)ve;
                vector4Field.SetValueWithoutNotify(new quaternion(m_XValue.GetValueFromBag(bag, cc), m_YValue.GetValueFromBag(bag, cc), m_ZValue.GetValueFromBag(bag, cc), m_WValue.GetValueFromBag(bag, cc)));
            }
        }

        public new static readonly string ussClassName = "unity-vector4-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        internal override FieldDescription[] DescribeFields()
        {
            return new FieldDescription[]
            {
                new FieldDescription("X", "unity-x-input", (quaternion r) => r.value.x, delegate(ref quaternion r, sfloat v)
                {
                    r.value.x = v;
                }),
                new FieldDescription("Y", "unity-y-input", (quaternion r) => r.value.y, delegate(ref quaternion r, sfloat v)
                {
                    r.value.y = v;
                }),
                new FieldDescription("Z", "unity-z-input", (quaternion r) => r.value.z, delegate(ref quaternion r, sfloat v)
                {
                    r.value.z = v;
                }),
                new FieldDescription("Z", "unity-z-input", (quaternion r) => r.value.w, delegate(ref quaternion r, sfloat v)
                {
                    r.value.w = v;
                }),
            };
        }

        public QuaternionField()
            : this(null) { }

        public QuaternionField(string label)
            : base(label, 4)
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
        }
    }
}