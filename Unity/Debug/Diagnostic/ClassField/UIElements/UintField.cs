using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class UintField : TextValueField<uint>
    {
        public new class UxmlFactory : UxmlFactory<UintField, UxmlTraits> { }
        public new class UxmlTraits : TextValueFieldTraits<uint, UxmlUintAttributeDescription> { }

        protected VisualElement VisualInput => (VisualElement)GetType()
            .GetProperty("visualInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(this, null);

        private class UintInput : TextValueInput
        {
            private UintField parentUintField => (UintField)base.parent;

            protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForInt;

            internal UintInput()
            {
                base.formatString = UINumericFieldsUtils.k_IntFieldFormatString;
            }

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, uint startValue)
            {
                double num = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue);
                float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                uint num2 = StringToValue(base.text);
                num2 += (uint)Math.Round((double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num);
                if (parentUintField.isDelayed)
                {
                    base.text = ValueToString(num2);
                }
                else
                {
                    parentUintField.value = num2;
                }
            }

            protected override string ValueToString(uint v)
                => v.ToString(base.formatString);

            protected override uint StringToValue(string str)
            {
                UINumericFieldsUtils.StringToUint(str, out var value);
                return value;
            }
        }

        public new static readonly string ussClassName = "unity-int-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        private UintInput uintInput => (UintInput)base.textInputBase;

        protected override string ValueToString(uint v)
            => v.ToString(base.formatString, CultureInfo.InvariantCulture.NumberFormat);

        protected override uint StringToValue(string str)
            => UINumericFieldsUtils.StringToUint(str, out var num) ? num : base.rawValue;

        public UintField()
            : this(null) { }

        public UintField(int maxLength)
            : this(null, maxLength) { }

        public UintField(string label, int maxLength = -1)
            : base(label, maxLength, new UintInput())
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
            AddLabelDragger<uint>();
        }

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, uint startValue)
        {
            uintInput.ApplyInputDeviceDelta(delta, speed, startValue);
        }
    }
}