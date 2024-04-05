using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct NullPresent
    {
        public VisualElement Create(ObjectProperty property)
            => new Label("<NULL>")
            {
                name = "present-field-null",
            };

        public void Set(ObjectProperty value, VisualElement container) { }
        public void Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange) { }
        public bool IsNull(VisualElement container)
            => container.name == "present-field-null";
    }
}

