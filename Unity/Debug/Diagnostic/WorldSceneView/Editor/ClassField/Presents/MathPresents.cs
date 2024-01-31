using System;
using AnotherECS.Debug.Diagnostic.Editor.UIElements;
using AnotherECS.Mathematics;
using UnityEngine.UIElements;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    internal struct SfloatPresent : IPresent
    {
        Type IPresent.Type => typeof(sfloat);
        VisualElement IPresent.Create(ObjectProperty property)
            => new SFloatField(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<SFloatField, sfloat>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<sfloat>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Float2Present : IPresent
    {
        Type IPresent.Type => typeof(float2);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Float2Field(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Float2Field, float2>(value, container);

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float2>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Float3Present : IPresent
    {
        Type IPresent.Type => typeof(float3);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Float3Field(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Float3Field, float3>(value, container);

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float3>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Float4Present : IPresent
    {
        Type IPresent.Type => typeof(float4);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Float4Field(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Float4Field, float4>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float4>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Int2Present : IPresent
    {
        Type IPresent.Type => typeof(int2);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Int2Field(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Int2Field, int2>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int2>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Int3Present : IPresent
    {
        Type IPresent.Type => typeof(int3);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Int3Field(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Int3Field, int3>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int3>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Int4Present : IPresent
    {
        Type IPresent.Type => typeof(int4);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Int4Field(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Int4Field, int4>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int4>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct QuaternionPresent : IPresent
    {
        Type IPresent.Type => typeof(quaternion);
        VisualElement IPresent.Create(ObjectProperty property)
            => new QuaternionField(property.GetFieldDisplayName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<QuaternionField, quaternion>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<quaternion>>((e) => onChange(property, e.previousValue, e.newValue));
    }
}

