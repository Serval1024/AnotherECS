using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class UlongField : TextValueField<ulong>
    {
        public new class UxmlFactory : UxmlFactory<UlongField, UxmlTraits> { }
        public new class UxmlTraits : TextValueFieldTraits<ulong, UxmlUlongAttributeDescription> { }

        protected VisualElement VisualInput => (VisualElement)GetType()
                .GetProperty("visualInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(this, null);

        private class UlongInput : TextValueInput
        {
            private UlongField parentUlongField => (UlongField)base.parent;

            protected override string allowedCharacters => UINumericFieldsUtils.k_AllowedCharactersForInt;

            internal UlongInput()
            {
                base.formatString = UINumericFieldsUtils.k_IntFieldFormatString;
            }

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ulong startValue)
            {
                double num = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue);
                float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                ulong num2 = StringToValue(base.text);
                num2 += (ulong)Math.Round((double)NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * num);
                if (parentUlongField.isDelayed)
                {
                    base.text = ValueToString(num2);
                }
                else
                {
                    parentUlongField.value = num2;
                }
            }

            protected override string ValueToString(ulong v)
                => v.ToString(base.formatString);

            protected override ulong StringToValue(string str)
            {
                UINumericFieldsUtils.StringToUlong(str, out var value);
                return value;
            }
        }

        public new static readonly string ussClassName = "unity-long-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        private UlongInput ulongInput => (UlongInput)base.textInputBase;

        protected override string ValueToString(ulong v)
            => v.ToString(base.formatString, CultureInfo.InvariantCulture.NumberFormat);

        protected override ulong StringToValue(string str)    
            => UINumericFieldsUtils.StringToUlong(str, out var num) ? num : base.rawValue;

        public UlongField()
            : this(null) { }

        public UlongField(int maxLength)
            : this(null, maxLength) { }

        public UlongField(string label, int maxLength = -1)
            : base(label, maxLength, new UlongInput())
        {
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
            AddLabelDragger<ulong>();
        }

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, ulong startValue)
        {
            ulongInput.ApplyInputDeviceDelta(delta, speed, startValue);
        }
    }
}