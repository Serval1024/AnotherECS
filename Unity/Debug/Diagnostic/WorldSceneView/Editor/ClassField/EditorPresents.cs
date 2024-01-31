using System;
using System.Collections;
using AnotherECS.Mathematics;
using AnotherECS.Debug.Diagnostic.Editor.UIElements;
using UnityEngine.UIElements;
using AnotherECS.Core;
using UnityEditor.UIElements;
using UnityEditor;

namespace AnotherECS.Unity.Debug.Diagnostic.Editor
{
    internal struct BoolPresent : IPresent
    {
        Type IPresent.Type => typeof(bool);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Toggle(property.GetFieldName());

        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Toggle, bool>(value, container);
            
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<bool>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct IntPresent : IPresent
    {
        Type IPresent.Type => typeof(int);
        VisualElement IPresent.Create(ObjectProperty property)
            => new IntegerField(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)            
            => PresentUtils.SetWithCheck<IntegerField, int>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct UintPresent : IPresent
    {
        Type IPresent.Type => typeof(uint);
        VisualElement IPresent.Create(ObjectProperty property)
            => new UintField(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<UintField, uint>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<uint>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct FloatPresent : IPresent
    {
        Type IPresent.Type => typeof(float);
        VisualElement IPresent.Create(ObjectProperty property)
            => new FloatField(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<FloatField, float>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct DoublePresent : IPresent
    {
        Type IPresent.Type => typeof(double);
        VisualElement IPresent.Create(ObjectProperty property)
            => new DoubleField(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<DoubleField, double>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<double>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct LongPresent : IPresent
    {
        Type IPresent.Type => typeof(long);
        VisualElement IPresent.Create(ObjectProperty property)
            => new LongField(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<LongField, long>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<long>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct UlongPresent : IPresent
    {
        Type IPresent.Type => typeof(ulong);
        VisualElement IPresent.Create(ObjectProperty property)
            => new UlongField(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<UlongField, ulong>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<ulong>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct SfloatPresent : IPresent
    {
        Type IPresent.Type => typeof(sfloat);
        VisualElement IPresent.Create(ObjectProperty property)
            => new SFloatField(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<SFloatField, sfloat>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<sfloat>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Float2Present : IPresent
    {
        Type IPresent.Type => typeof(float2);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Float2Field(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Float2Field, float2>(value, container);

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float2>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Float3Present : IPresent
    {
        Type IPresent.Type => typeof(float3);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Float3Field(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Float3Field, float3>(value, container);

        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float3>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Float4Present : IPresent
    {
        Type IPresent.Type => typeof(float4);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Float4Field(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Float4Field, float4>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<float4>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Int2Present : IPresent
    {
        Type IPresent.Type => typeof(int2);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Int2Field(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Int2Field, int2>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int2>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Int3Present : IPresent
    {
        Type IPresent.Type => typeof(int3);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Int3Field(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Int3Field, int3>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int3>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct Int4Present : IPresent
    {
        Type IPresent.Type => typeof(int4);
        VisualElement IPresent.Create(ObjectProperty property)
            => new Int4Field(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<Int4Field, int4>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<int4>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct QuaternionPresent : IPresent
    {
        Type IPresent.Type => typeof(quaternion);
        VisualElement IPresent.Create(ObjectProperty property)
            => new QuaternionField(property.GetFieldName());
        void IPresent.Set(ObjectProperty value, VisualElement container)
            => PresentUtils.SetWithCheck<QuaternionField, quaternion>(value, container);
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
            => container.RegisterCallback<ChangeEvent<quaternion>>((e) => onChange(property, e.previousValue, e.newValue));
    }

    internal struct EntityPresent : IPresent
    {
        Type IPresent.Type => typeof(Entity);
        VisualElement IPresent.Create(ObjectProperty property)
        {
            var container = PresentUtils.CreateHorizontalGroup(null);
            var content = container.Q("group-content");
            container.Add(content);

            IPresent present = new UintPresent();
            var idView = present.Create(property.GetPrivateChild("id"));
            idView.name = "entity__input";
            idView.style.flexGrow = new StyleFloat(1f);

            content.Add(idView);

            var button = new Button
            {
                text = "↗",
                name = "entity__button"
            };

            button.clicked += ()=> OnClickButton(container, (Entity)property.GetValue());
            button.schedule.Execute(() => button.SetEnabled(((Entity)property.GetValue()).IsValid)).Every(100);

            content.Add(button);

            return container;
        }

        void IPresent.Set(ObjectProperty value, VisualElement container)
        {
            IPresent present = new UintPresent();
            present.Set(value.GetPrivateChild("id"), container.Q("entity__input"));

        }
        void IPresent.Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange)
        {
            IPresent present = new UintPresent();
            present.Register(property.GetPrivateChild("id"), container.Q("entity__input"), onChange);
        }

        private static void OnClickButton(VisualElement container, Entity entity)
        {
            if (entity.IsValid)
            {
                using EntityLocatedButtonEvent @event = EntityLocatedButtonEvent.GetPooled();
                @event.target = container;
                @event.id = entity.id;
                container.SendEvent(@event);
            }
        }



        public class EntityLocatedButtonEvent : EventBase<EntityLocatedButtonEvent>
        {
            public uint id;

            public EntityLocatedButtonEvent() => LocalInit();

            protected override void Init()
            {
                base.Init();
                LocalInit();
            }

            private void LocalInit()
            {
                bubbles = true;
            }
        }
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
            var container = PresentUtils.CreateGroup(property.GetFieldName());
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

