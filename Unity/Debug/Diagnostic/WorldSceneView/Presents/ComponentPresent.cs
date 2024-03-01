using AnotherECS.Core;
using System;
using UnityEngine.UIElements;
using EntityId = System.UInt32;

namespace AnotherECS.Unity.Debug.Diagnostic.Present
{
    internal struct ComponentPresent : IPresent
    {
        Type IPresent.Type => typeof(IComponent);

        public VisualElement Create(ObjectProperty property)
        {
            IPresent compositePresent = new CompositePresent();

            var container = PresentUtils.CreateHorizontal(null);
            var content = container.Q("group-content");

            var element = compositePresent.Create(property);
            element.style.flexGrow = 1;
            content.Add(element);

            var space = new VisualElement();
            space.AddToClassList("horizontal-space-small");
            content.Add(space);
            
            if (property.GetPathIterator().IsIndex())
            {
                var id = property.GetPathIterator().GetIndex();
                var entityId = property.GetUserData<ComponentComponentPresentData>().entityId;                
                if (entityId != 0)
                {
                    var button = new Button
                    {
                        text = "x",
                        name = "component__remove-button"
                    };
                    content.Add(button);

                    button.clicked += () => OnClickButton(button, entityId, (uint)id);
                }
            }
            return content;
        }

        public void Set(ObjectProperty property, VisualElement container)
        {
            IPresent compositePresent = new CompositePresent();
            compositePresent.Set(property, container.Q("group-content").ElementAt(0));
        }

        public void Register(ObjectProperty property, VisualElement container, Action<ObjectProperty, object, object> onChange) { }

        private static void OnClickButton(VisualElement container, EntityId entityId, uint componentIndex)
        {
            if (entityId != 0)
            {
                using RemoveComponentButtonEvent @event = RemoveComponentButtonEvent.GetPooled();
                @event.target = container;
                @event.entityId = entityId;
                @event.componentIndex = componentIndex;
                container.SendEvent(@event);
            }
        }

        public struct ComponentComponentPresentData
        {
            public EntityId entityId;
        }

        public class RemoveComponentButtonEvent : EventBase<RemoveComponentButtonEvent>
        {
            public EntityId entityId;
            public uint componentIndex;

            public RemoveComponentButtonEvent() => LocalInit();

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

