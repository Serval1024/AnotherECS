using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    internal struct UnknowPresent
    {
        public VisualElement Create(ObjectProperty property)
            => GetPresent(property).Create(property);
        public void Set(ObjectProperty value, VisualElement container)
            => GetPresent(value.GetFieldType()).Set(value, container);
        public void Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => GetPresent(property).Register(property, container, onChange);

        private IPresent GetPresent(ObjectProperty property)
            => GetPresent(property.GetFieldType());

        private IPresent GetPresent(Type type) 
            => EditorPresentGlobalRegister.Get(type) ?? new CompositePresent();
    }
}

