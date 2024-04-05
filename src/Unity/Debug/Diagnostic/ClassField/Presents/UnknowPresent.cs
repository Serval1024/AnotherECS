using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct UnknowPresent
    {
        public VisualElement Create(ObjectProperty property)
        {
            if (property.IsValueType())
            {
                return GetPresent(property).Create(property);
            }
            else
            {
                var container = new VisualElement();

                container.Add(
                    property.IsValueNull()
                       ? default(NullPresent).Create(property)
                       : GetPresent(property).Create(property)
                   );
                return container;
            }
        }

        public void Set(ObjectProperty property, VisualElement container)
        {
            if (property.IsValueType())
            {
                GetPresent(property.GetFieldType()).Set(property, container);
            }
            else
            {
                NullPresent _nullPresent;
                var nullContainer = container.ElementAt(0);
                var isNull = _nullPresent.IsNull(nullContainer);

                if (property.IsValueNull() && isNull)
                {
                    _nullPresent.Set(property, nullContainer);
                }
                else if (!property.IsValueNull() && isNull)
                {
                    container.Clear();
                    container.Add(GetPresent(property).Create(property));
                }
                else if (property.IsValueNull() && !isNull)
                {
                    container.Clear();
                    container.Add(default(NullPresent).Create(property));
                }
                else if (!property.IsValueNull() && !isNull)
                {
                    GetPresent(property.GetFieldType()).Set(property, nullContainer);
                }
            }
        }
        public void Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
        {
            if (property.IsValueType())
            {
                GetPresent(property).Register(property, container, onChange);
            }
            else
            {
                var nullContainer = container.ElementAt(0);

                if (property.IsValueNull())
                {
                    default(NullPresent).Register(property, nullContainer, onChange);
                }
                else
                {
                    GetPresent(property).Register(property, nullContainer, onChange);
                }
            }
        }

        private IPresent GetPresent(ObjectProperty property)
            => GetPresent(property.GetFieldType());

        private IPresent GetPresent(Type type) 
            => EditorPresentGlobalRegister.Get(type) ?? new CompositePresent();
    }
}

