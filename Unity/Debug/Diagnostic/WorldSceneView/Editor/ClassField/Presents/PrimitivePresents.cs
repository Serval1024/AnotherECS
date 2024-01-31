using System;
using System.Collections;
using AnotherECS.Debug.Diagnostic.Editor.UIElements;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    internal struct BoolPresent : IPresent
    {
        Type IPresent.Type => typeof(bool);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Toggle(property.GetFieldDisplayName());

        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Toggle, bool>(value, container);
            
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<bool>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct IntPresent : IPresent
    {
        Type IPresent.Type => typeof(int);
        VisualElement IPresent.Create(ObjectProperty property)
            => new IntegerField(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)            
            => PresentUtils.SetWithCheck<IntegerField, int>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct UintPresent : IPresent
    {
        Type IPresent.Type => typeof(uint);
        VisualElement IPresent.Create(ObjectProperty property)
            => new UintField(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<UintField, uint>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<uint>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct FloatPresent : IPresent
    {
        Type IPresent.Type => typeof(float);
        VisualElement IPresent.Create(ObjectProperty property)
            => new FloatField(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<FloatField, float>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct DoublePresent : IPresent
    {
        Type IPresent.Type => typeof(double);
        VisualElement IPresent.Create(ObjectProperty property)
            => new DoubleField(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<DoubleField, double>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<double>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct LongPresent : IPresent
    {
        Type IPresent.Type => typeof(long);
        VisualElement IPresent.Create(ObjectProperty property)
            => new LongField(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<LongField, long>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<long>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct UlongPresent : IPresent
    {
        Type IPresent.Type => typeof(ulong);
        VisualElement IPresent.Create(ObjectProperty property)
            => new UlongField(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<UlongField, ulong>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<ulong>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct UnknowPresent : IPresent
    {
        Type IPresent.Type => typeof(object);
        VisualElement IPresent.Create(ObjectProperty property)
            => GetPresent(property).Create(property);
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => GetPresent(value.GetFieldType()).Set(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => GetPresent(property).Register(property, container, onChange);

        private IPresent GetPresent(ObjectProperty property)
            => GetPresent(property.GetFieldType());

        private IPresent GetPresent(Type type) 
            => EditorPresentGlobalRegister.Get(type) ?? new CompositePresent();
    }

    internal struct CompositePresent : IPresent
    {
        Type IPresent.Type => typeof(IEnumerable);
        VisualElement IPresent.Create(ObjectProperty property)
        {
            var container = PresentUtils.CreateGroupBox(property.GetFieldDisplayName());
            var content = container.Q("group-content");

            IPresent unknowPresent = new UnknowPresent();
            foreach (var child in property.GetChildren())
            {
                content.Add(unknowPresent.Create(child));
            }

            return container;
        }

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
        {
            IPresent unknowPresent = new UnknowPresent();
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
            IPresent unknowPresent = new UnknowPresent();
            var content = container.Q("group-content");

            int index = 0;
            foreach (var child in property.GetChildren())
            {
                unknowPresent.Set(child, content.ElementAt(index));
                ++index;
            }
        }
    }
}

