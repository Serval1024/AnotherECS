using AnotherECS.Collections;
using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct StringPresent : IPresent
    {
        Type IPresent.Type => typeof(string);

        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<TextField, string>(ref property);

        void IPresent.Set(ObjectProperty property, VisualElement container)
        {
            Set(property.GetValue<string>(), container);
        }

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<string>>((e) => onChange(property, e.previousValue, e.newValue));

        public static void Set(string value, VisualElement container)
        {   
            var textValueField = (BaseField<string>)container;
            if (textValueField.value != value)
            {
                textValueField.SetValueWithoutNotify(value);
            }
        }
    }

    internal struct CStringPresent : IPresent
    {
        Type IPresent.Type => typeof(ICString<char>);

        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<TextField, string>(ref property);

        void IPresent.Set(ObjectProperty property, VisualElement container)
        {
            StringPresent.Set(property.GetValue<ICString<char>>().ToString(), container);
        }

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<string>>((e) => onChange(property, e.previousValue, e.newValue));
    }
}

