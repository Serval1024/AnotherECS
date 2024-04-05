using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class SFloatField : TextValueField<sfloat>
    {
        public new class UxmlFactory : UxmlFactory<SFloatField, UxmlTraits> { }
        public new class UxmlTraits : TextValueFieldTraits<sfloat, UxmlSfloatAttributeDescription> { }

        private class SFloatInput : TextValueInput
        {
            private SFloatField parentSfloatField => (SFloatField)base.parent;

            protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForFloat;

            internal SFloatInput()
            {
                base.formatString = UINumericFieldsUtils.k_IntFieldFormatString;
            }

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, sfloat startValue)
            {
                float num = (float)NumericFieldDraggerUtility.CalculateFloatDragSensitivity((double)(float)startValue);
                float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                sfloat num2 = StringToValue(base.text);
                num2 += (sfloat)Math.Round((double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num);
                if (parentSfloatField.isDelayed)
                {
                    base.text = ValueToString(num2);
                }
                else
                {
                    parentSfloatField.value = num2;
                }
            }

            protected override string ValueToString(sfloat v)
                => v.ToString(base.formatString);

            protected override sfloat StringToValue(string str)
            {
                UINumericFieldsUtils.StringToSfloat(str, out var value);
                return value;
            }
        }

        public new static readonly string ussClassName = "unity-float-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        protected VisualElement VisualInput => (VisualElement)GetType()
                .GetProperty("visualInput", ReflectionUtils.nonPublicFlags)
                .GetValue(this, null);

        private SFloatInput sfloatInput => (SFloatInput)base.textInputBase;

        protected override string ValueToString(sfloat v)
            => v.ToString(base.formatString, CultureInfo.InvariantCulture.NumberFormat);

        protected override sfloat StringToValue(string str)
            => UINumericFieldsUtils.StringToSfloat(str, out var num) ? num : base.rawValue;

        public SFloatField()
            : this(null) { }

        public SFloatField(int maxLength)
            : this(null, maxLength) { }

        public SFloatField(string label, int maxLength = -1)
            : base(label, maxLength, new SFloatInput())
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
            AddLabelDragger<sfloat>();
        }

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, sfloat startValue)
        {
            sfloatInput.ApplyInputDeviceDelta(delta, speed, startValue);
        }
    }
}