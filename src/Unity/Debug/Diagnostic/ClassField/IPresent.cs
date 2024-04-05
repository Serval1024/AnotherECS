using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal interface IPresent
    {
        int Priority { get => 0; }
        Type Type { get; }
        VisualElement Create(ObjectProperty property);
        void Set(ObjectProperty property, VisualElement container);
        void Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange);
    }
}

