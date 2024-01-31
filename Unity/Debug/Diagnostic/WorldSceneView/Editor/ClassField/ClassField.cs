using System;
using System.Collections.Generic;
using AnotherECS.Unity.Debug.Diagnostic;
using AnotherECS.Unity.Debug.Diagnostic.Editor;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.Editor.UIElements
{
    public class ClassField : VisualElement
    {
        public static readonly string ussClassName = "unity-property-field";
        public static readonly string labelUssClassName = ussClassName + "__label";
        public static readonly string inputUssClassName = ussClassName + "__input";
        public static readonly string noLabelVariantUssClassName = ussClassName + "--no-label";

        private object _value;
        private event Action<ObjectProperty, object, object> _changed;

        private IPresent _unknowPresent;

        public Label labelElement { get; private set; }
        public VisualElement inputElement { get; private set; }

        public string label
        {
            get
            {
                return labelElement.text;
            }
            set
            {
                if (labelElement.text != value)
                {
                    labelElement.text = value;
                    if (string.IsNullOrEmpty(labelElement.text))
                    {
                        AddToClassList(noLabelVariantUssClassName);
                        labelElement.RemoveFromHierarchy();
                    }
                    else if (!Contains(labelElement))
                    {
                        Insert(0, labelElement);
                        RemoveFromClassList(noLabelVariantUssClassName);
                    }
                }
            }
        }

        public object value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!EqualityComparer<object>.Default.Equals(_value, value))
                {
                    if (base.panel != null)
                    {
                        var previousValue = _value;
                        SetValueWithoutNotify(value);
                        OnChange(new ObjectProperty(_value), previousValue, _value);
                    }
                    else
                    {
                        SetValueWithoutNotify(value);
                    }
                }
            }
        }

        public ClassField()
           : this(string.Empty) { }

        public ClassField(string label)
        {
            base.focusable = true;
            base.tabIndex = 0;
            base.delegatesFocus = true;
            AddToClassList(ussClassName);
            labelElement = new Label
            {
                focusable = true,
                tabIndex = -1
            };
            labelElement.AddToClassList(labelUssClassName);
            if (label != null)
            {
                this.label = label;
            }
            else
            {
                AddToClassList(noLabelVariantUssClassName);
            }

            _unknowPresent = new UnknowPresent();
        }

        public void SetValueWithoutNotify(object newValue)
        {
            if (_value == null || _value.GetType() != newValue.GetType())
            {
                inputElement?.RemoveFromHierarchy();

                var objectProperty = new ObjectProperty(newValue);
                var view = _unknowPresent.Create(objectProperty);
                view.AddToClassList(inputUssClassName);
                Add(view);
                _unknowPresent.Set(objectProperty, view);
                _unknowPresent.Register(objectProperty, view, OnChange);

                inputElement = view;
            }
            else
            {
                var objectProperty = new ObjectProperty(newValue);
                _unknowPresent.Set(objectProperty, inputElement);
            }

            _value = newValue;
        }

        public void RegisterValueChangeCallback(Action<ObjectProperty, object, object> callback)
        {
            _changed += callback;
        }

        public void UnregisterValueChangeCallback(Action<ObjectProperty, object, object> callback)
        {
            _changed -= callback;
        }

        private void OnChange(ObjectProperty property, object previousValue, object value)
        {
            using ChangeEvent changeEvent = ChangeEvent.GetPooled();
            changeEvent.property = property;
            changeEvent.previousValue = previousValue;
            changeEvent.value = value;
            changeEvent.target = this;
            SendEvent(changeEvent);

            _changed?.Invoke(property, previousValue, value);
        }

        public class ChangeEvent : EventBase<ChangeEvent>
        {
            public ObjectProperty property;
            public object previousValue;
            public object value;

            public ChangeEvent() => LocalInit();

            protected override void Init()
            {
                base.Init();
                LocalInit();
            }

            private void LocalInit()
            {
                bubbles = true;
            }
        }
    }
}