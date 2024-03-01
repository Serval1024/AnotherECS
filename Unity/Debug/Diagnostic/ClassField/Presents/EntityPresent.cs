using AnotherECS.Core;
using System;
using UnityEngine.UIElements;
using EntityId = System.UInt32;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct EntityPresent : IPresent
    {
        Type IPresent.Type => typeof(Entity);

        VisualElement IPresent.Create(ObjectProperty property)
        {
            var container = PresentUtils.CreateHorizontal(null);
            var content = container.Q("group-content");
            container.Add(content);

            IPresent present = new UintPresent();
            var propertyId = property
                .GetPrivateChild("id")
                .ToFieldDisplayName(property.GetFieldDisplayName());
            
            var idView = present.Create(propertyId);
            idView.name = "entity__input";
            idView.style.flexGrow = new StyleFloat(1f);

            content.Add(idView);

            var button = new Button
            {
                text = "↗",
                name = "entity__button"
            };

            button.clicked += ()=> OnClickButton(button, property.GetValue<Entity>());
            button.schedule.Execute(() => button.SetEnabled(property.GetValue<Entity>().IsValid)).Every(100);

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
            present.Register(
                property.GetPrivateChild("id"),
                container.Q("entity__input"),
                (op, pv, v) => onChange(property, EntityExtensions.CreateRaw((EntityId)pv), EntityExtensions.CreateRaw((EntityId)v))
                );
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
}

