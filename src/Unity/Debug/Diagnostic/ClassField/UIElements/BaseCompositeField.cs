using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public abstract class BaseCompositeField<TValueType, TField, TFieldValue> : BaseField<TValueType>
        where TField : TextValueField<TFieldValue>, new()
    {
        internal struct FieldDescription
        {
            public delegate void WriteDelegate(ref TValueType val, TFieldValue fieldValue);

            internal readonly string name;

            internal readonly string ussName;

            internal readonly Func<TValueType, TFieldValue> read;

            internal readonly WriteDelegate write;

            public FieldDescription(string name, string ussName, Func<TValueType, TFieldValue> read, WriteDelegate write)
            {
                this.name = name;
                this.ussName = ussName;
                this.read = read;
                this.write = write;
            }
        }

        private List<TField> m_Fields;

        private bool m_ShouldUpdateDisplay;

        private bool m_ForceUpdateDisplay;

        private int m_PropertyIndex;

        public new static readonly string ussClassName = "unity-composite-field";

        public new static readonly string labelUssClassName = ussClassName + "__label";

        public new static readonly string inputUssClassName = ussClassName + "__input";

        public static readonly string spacerUssClassName = ussClassName + "__field-spacer";

        public static readonly string multilineVariantUssClassName = ussClassName + "--multi-line";

        public static readonly string fieldGroupUssClassName = ussClassName + "__field-group";

        public static readonly string fieldUssClassName = ussClassName + "__field";

        public static readonly string firstFieldVariantUssClassName = fieldUssClassName + "--first";

        public static readonly string twoLinesVariantUssClassName = ussClassName + "--two-lines";

        internal List<TField> fields => m_Fields;

        protected VisualElement VisualInput => (VisualElement)GetType()
                .GetProperty("visualInput", ReflectionUtils.nonPublicFlags)
                .GetValue(this, null);

        internal int propertyIndex
        {
            get
            {
                return m_PropertyIndex;
            }
            set
            {
                m_PropertyIndex = value;
            }
        }

        internal bool forceUpdateDisplay
        {
            get
            {
                return m_ForceUpdateDisplay;
            }
            set
            {
                m_ForceUpdateDisplay = value;
            }
        }

        public bool isDelayed
        {
            get
            {
                bool summary = true;
                for (int i = 0; i < m_Fields.Count; ++i)
                {
                    summary &= m_Fields[i].isDelayed;
                }
                return summary;
            }
            set
            {
                for (int i = 0; i < m_Fields.Count; ++i)
                {
                    m_Fields[i].isDelayed = value;
                }
            }
        }

        private VisualElement GetSpacer()
        {
            VisualElement visualElement = new VisualElement();
            visualElement.AddToClassList(spacerUssClassName);
            visualElement.visible = false;
            visualElement.focusable = false;
            return visualElement;
        }

        internal abstract FieldDescription[] DescribeFields();

        protected BaseCompositeField(string label, int fieldsByLine)
            : base(label, (VisualElement)null)
        {
            base.delegatesFocus = false;
            VisualInput.focusable = false;
            AddToClassList(ussClassName);
            base.labelElement.AddToClassList(labelUssClassName);
            VisualInput.AddToClassList(inputUssClassName);
            m_ShouldUpdateDisplay = true;
            m_Fields = new List<TField>();
            FieldDescription[] array = DescribeFields();
            int num = 1;
            if (fieldsByLine > 1)
            {
                num = array.Length / fieldsByLine;
            }

            bool flag = false;
            if (num > 1)
            {
                flag = true;
                AddToClassList(multilineVariantUssClassName);
            }

            m_PropertyIndex = 0;
            for (int i = 0; i < num; i++)
            {
                VisualElement visualElement = null;
                if (flag)
                {
                    visualElement = new VisualElement();
                    visualElement.AddToClassList(fieldGroupUssClassName);
                }

                bool flag2 = true;
                for (int j = i * fieldsByLine; j < i * fieldsByLine + fieldsByLine; j++)
                {
                    FieldDescription desc = array[j];
                    TField field = new TField
                    {
                        name = desc.ussName
                    };
                    field.delegatesFocus = true;
                    field.AddToClassList(fieldUssClassName);
                    if (flag2)
                    {
                        field.AddToClassList(firstFieldVariantUssClassName);
                        flag2 = false;
                    }

                    field.label = desc.name;

                    Func<TFieldValue, TFieldValue> func = (TFieldValue newValue) =>
                    {
                        TValueType val2 = value;
                        desc.write(ref val2, newValue);
                        TValueType arg = ValidatedValue(val2);
                        return desc.read(arg);
                    };
                    AddEventOnValidateValue(field, func);

                    field.RegisterValueChangedCallback(delegate (ChangeEvent<TFieldValue> e)
                    {
                        TValueType val = value;
                        desc.write(ref val, e.newValue);
                        string text = e.newValue.ToString();
                        string text2 = ((TField)e.currentTarget).text;
                        if (text != text2 || CanTryParse(text2))
                        {
                            m_ShouldUpdateDisplay = false;
                        }

                        value = val;
                        m_ShouldUpdateDisplay = true;
                    });
                    m_Fields.Add(field);
                    if (flag)
                    {
                        visualElement.Add(field);
                    }
                    else
                    {
                        VisualInput.hierarchy.Add(field);
                    }
                }

                if (fieldsByLine < 3)
                {
                    int num2 = 3 - fieldsByLine;
                    for (int k = 0; k < num2; k++)
                    {
                        if (flag)
                        {
                            visualElement.Add(GetSpacer());
                        }
                        else
                        {
                            VisualInput.hierarchy.Add(GetSpacer());
                        }
                    }
                }

                if (flag)
                {
                    VisualInput.hierarchy.Add(visualElement);
                }
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (m_Fields.Count != 0)
            {
                int num = 0;
                FieldDescription[] array = DescribeFields();
                FieldDescription[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    FieldDescription fieldDescription = array2[i];
                    m_Fields[num].value = fieldDescription.read(base.rawValue);
                    num++;
                }
            }
        }

        public override void SetValueWithoutNotify(TValueType newValue)
        {
            bool flag = m_ForceUpdateDisplay || (m_ShouldUpdateDisplay && !EqualityComparer<TValueType>.Default.Equals(base.rawValue, newValue));
            base.SetValueWithoutNotify(newValue);
            if (flag)
            {
                UpdateDisplay();
            }

            m_ForceUpdateDisplay = false;
        }
        
        protected override void UpdateMixedValueContent() { }


        private bool CanTryParse(string text)
            => (bool)GetType()
            .GetMethod("CanTryParse", ReflectionUtils.nonPublicFlags)
            .Invoke(this, new object[] { text });

        private void AddEventOnValidateValue(TextValueField<TFieldValue> field, Func<TFieldValue, TFieldValue> func)
        {
            field.GetType()
                .GetEvent("onValidateValue", ReflectionUtils.nonPublicFlags)
                .GetAddMethod(true).Invoke(field, new object[] { func });
        }

        private TValueType ValidatedValue(TValueType value)
            => (TValueType)GetType()
                .GetMethod("ValidatedValue", ReflectionUtils.nonPublicFlags)
                .Invoke(this, new object[] { value });
    }
}