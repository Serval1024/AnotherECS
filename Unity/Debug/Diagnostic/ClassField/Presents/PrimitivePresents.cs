using AnotherECS.Debug.Diagnostic.UIElements;
using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct BoolPresent : IPresent
    {
        Type IPresent.Type => typeof(bool);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateFieldBool(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<Toggle, bool>(ref property, container);     
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<bool>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct IntPresent : IPresent
    {
        Type IPresent.Type => typeof(int);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<IntegerField, int>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)            
            => PresentUtils.SetWithCheck<IntegerField, int>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct UintPresent : IPresent
    {
        Type IPresent.Type => typeof(uint);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<UintField, uint>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<UintField, uint>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<uint>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct FloatPresent : IPresent
    {
        Type IPresent.Type => typeof(float);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<FloatField, float>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<FloatField, float>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct DoublePresent : IPresent
    {
        Type IPresent.Type => typeof(double);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<DoubleField, double>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<DoubleField, double>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<double>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct LongPresent : IPresent
    {
        Type IPresent.Type => typeof(long);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<LongField, long>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<LongField, long>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<long>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct UlongPresent : IPresent
    {
        Type IPresent.Type => typeof(ulong);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<UlongField, ulong>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<UlongField, ulong>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<ulong>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct EnumPresent : IPresent
    {
        Type IPresent.Type => typeof(Enum);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateFieldEnum(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<EnumField, Enum>(ref property, container);   
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange) 
            => container.RegisterCallback<ChangeEvent<Enum>>((e) => onChange(property, e.previousValue, e.newValue));
    }
}

