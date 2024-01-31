using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    internal interface IPresent
    {
        Type Type { get; }
        VisualElement Create(ObjectProperty property);
        void Set(ObjectProperty value, VisualElement container);
        void Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange);
    }
}

