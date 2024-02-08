using System;
using System.Collections;
using System.Linq;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct CompositePresent : IPresent
    {
        int IPresent.Priority { get => -1; }
        Type IPresent.Type => typeof(IEnumerable);
        VisualElement IPresent.Create(ObjectProperty property)
        {
            var container = PresentUtils.CreateGroup(property.GetFieldDisplayName());
            var content = container.Q("group-content");

            UnknowPresent unknowPresent;
            foreach (var child in property.GetChildren())
            {
                content.Add(unknowPresent.Create(child));
            }

            return container;
        }

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
        {
            UnknowPresent unknowPresent;
            var content = container.Q("group-content");

            int index = 0;
            foreach (var child in property.GetChildren())
            {
                unknowPresent.Register(child, content.ElementAt(index), onChange);
                ++index;
            }
        }

        void IPresent.Set(ObjectProperty property, VisualElement container)
        {
            UnknowPresent unknowPresent;
            var content = container.Q("group-content");

            var count = property.ChildCount();

            if (count < content.childCount)
            {
                for (int i = count; i < content.childCount; ++i)
                {
                    content.ElementAt(i).RemoveFromHierarchy();
                }
            }
            else if (count > content.childCount)
            {
                foreach (var child in property.GetChildren().Skip(content.childCount))
                {
                    content.Add(unknowPresent.Create(child));
                }
            }

            int index = 0;
            foreach (var child in property.GetChildren())
            {
                unknowPresent.Set(child, content.ElementAt(index));
                ++index;
            }
        }
    }
}

