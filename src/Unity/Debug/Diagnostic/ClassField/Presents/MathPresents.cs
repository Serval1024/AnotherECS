using AnotherECS.Debug.Diagnostic.UIElements;
using AnotherECS.Mathematics;
using System;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct SfloatPresent : IPresent
    {
        Type IPresent.Type => typeof(sfloat);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<SFloatField, sfloat>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<SFloatField, sfloat>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<sfloat>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Float2Present : IPresent
    {
        Type IPresent.Type => typeof(float2);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<Float2Field, float2, SFloatField, sfloat>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<Float2Field, float2>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float2>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Float3Present : IPresent
    {
        Type IPresent.Type => typeof(float3);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<Float3Field, float3, SFloatField, sfloat>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<Float3Field, float3>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float3>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Float4Present : IPresent
    {
        Type IPresent.Type => typeof(float4);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<Float4Field, float4, SFloatField, sfloat>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<Float4Field, float4>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float4>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Int2Present : IPresent
    {
        Type IPresent.Type => typeof(int2);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<Int2Field, int2, IntegerField, int>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<Int2Field, int2>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int2>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Int3Present : IPresent
    {
        Type IPresent.Type => typeof(int3);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<Int3Field, int3, IntegerField, int>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<Int3Field, int3>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int3>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Int4Present : IPresent
    {
        Type IPresent.Type => typeof(int4);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<Int4Field, int4, IntegerField, int>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<Int4Field, int4>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int4>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct QuaternionPresent : IPresent
    {
        Type IPresent.Type => typeof(quaternion);
        VisualElement IPresent.Create(ObjectProperty property)
            => PresentUtils.CreateField<QuaternionField, quaternion, SFloatField, sfloat>(ref property);
        void IPresent.Set(ObjectProperty property, VisualElement container)
            => PresentUtils.SetWithCheck<QuaternionField, quaternion>(ref property, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<quaternion>>((e) => onChange(property, e.previousValue, e.newValue));
    }
}

