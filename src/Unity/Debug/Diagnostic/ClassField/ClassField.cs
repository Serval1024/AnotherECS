using AnotherECS.Unity.Debug.Diagnostic;
using AnotherECS.Unity.Debug.Diagnostic.Present;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    public class ClassField : VisualElement
    {
        public static readonly string ussClassName = "unity-property-field";
        public static readonly string labelUssClassName = ussClassName + "__label";
        public static readonly string inputUssClassName = ussClassName + "__input";
        public static readonly string noLabelVariantUssClassName = ussClassName + "--no-label";

        private object _value;
        private Option _option;
        private readonly HashSet<object> _userData;
        private readonly UnknowPresent _unknowPresent;

        private event Action<ObjectProperty, object, object> _changed;

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

        public ClassField(Option option = Option.None, HashSet<object> userData = null)
           : this(string.Empty, option, userData) { }

        public ClassField(string label, Option option = Option.None, HashSet<object> userData = null)
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

            _option = option;
            _userData = userData;
        }

        public void SetValueWithoutNotify(object value)
        {
            if (value != null)
            {
                if (_value == null || _value.GetType() != value.GetType())
                {
                    inputElement?.RemoveFromHierarchy();

                    var objectProperty = new ObjectProperty(value, _userData);

                    if (_option.HasFlag(Option.SkipFirstLabel))
                    {
                        objectProperty = objectProperty.ToFieldDisplayName(string.Empty);
                    }

                    var view = _unknowPresent.Create(objectProperty);
                    view.AddToClassList(inputUssClassName);
                    Add(view);
                    _unknowPresent.Set(objectProperty, view);
                    _unknowPresent.Register(objectProperty, view, OnChange);

                    inputElement = view;
                }
                else
                {
                    var objectProperty = new ObjectProperty(value, _userData);
                    _unknowPresent.Set(objectProperty, inputElement);
                }
            }
            else
            {
                inputElement?.RemoveFromHierarchy();
                inputElement = null;
            }
            _value = value;
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

        public enum Option
        {
            None,
            SkipFirstLabel,
        }
    }
}