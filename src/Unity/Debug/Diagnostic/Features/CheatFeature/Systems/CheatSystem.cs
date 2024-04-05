using AnotherECS.Collections;
using AnotherECS.Core;
using AnotherECS.Debug;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Unity.Debug.Diagnostic
{
    public struct CheatSystem : IReceiverSystem<CheatEvent>
    {
        public void Receive(State state, CheatEvent @event)
        {
            switch (@event.command)
            {
                case CheatEvent.Command.AddEntity:
                    {
                        state.New();
                        break;
                    }
                case CheatEvent.Command.RemoveEntity:
                    {
                        if (@event.id != 0 && state.IsHas(@event.id))
                        {
                            state.Delete(@event.id);
                        }
                        break;
                    }
                case CheatEvent.Command.AddComponent:
                    {
                        if (@event.id != 0 && state.IsHas(@event.id))
                        {
                            if (@event.value is IComponent iComponent)
                            {
                                if (!state.IsHas(@event.id, iComponent.GetType()))
                                {
                                    state.Add(@event.id, iComponent);
                                }
                            }
                        }
                        break;
                    }
                case CheatEvent.Command.RemoveComponent:
                    {
                        if (@event.id != 0 && state.IsHas(@event.id))
                        {
                            if (state.IsHas(@event.id, @event.componentIndex))
                            {
                                state.Remove(@event.id, @event.componentIndex);
                            }
                        }
                        break;
                    }
                case CheatEvent.Command.ChangeComponent:
                    {
                        if (@event.id != 0 && state.IsHas(@event.id) && state.IsHas(@event.id, @event.componentIndex))
                        {
                            var component = state.Read(@event.id, @event.componentIndex);
                            try
                            {
                                var property = new ObjectProperty(component, @event.pathInsideComponent);
                                property.SetValue(TryConvertValue(state, property.GetValue(), @event.value));
                                component = property.GetRoot().GetValue<IComponent>();
                            }
                            catch
                            {
                                Logger.Send($"[{nameof(CheatSystem)}] Broken path: '{@event.pathInsideComponent}'. Component type: {component.GetType().Name}.");
                                return;
                            }
                            state.Set(@event.id, @event.componentIndex, component);
                        }
                        break;
                    }
            }
        }

        private object TryConvertValue(State state, object oldValue, object newValue)
        {
            if (newValue is Entity newValueEntity)
            {
                return state.ToEntity(newValueEntity.id);
            }
            else if (newValue is string newValueString && oldValue is ICString<char> oldValueCString)
            {
                oldValueCString.Set(newValueString);
                return oldValueCString;
            }

            return newValue;
        }
    }

    public struct CheatEvent : IEvent, ISerialize
    {
        public Command command;

        public EntityId id;
        public uint componentIndex;

        public string pathInsideComponent;
        public object value;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(command);

            writer.Write(id);
            writer.Write(componentIndex);
            writer.Write(pathInsideComponent);
            writer.Pack(value);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            command = reader.ReadEnum<Command>();

            id = reader.ReadUInt32();
            componentIndex = reader.ReadUInt32();
            pathInsideComponent = reader.ReadString();
            value = reader.Unpack();
        }

        public enum Command
        {
            AddEntity,
            RemoveEntity,

            AddComponent,
            RemoveComponent,
            ChangeComponent,
        }
    }
}
