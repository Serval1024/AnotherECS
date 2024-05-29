using AnotherECS.Serializer;
using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Remote
{
    public struct WorldData : ISerialize
    {
        public DataType Type { get; private set; }
        public IEnumerable<ISystem> Systems { get; private set; }
        public State State { get; private set; }

        public WorldData(ISystem system, State state)
          : this(new SystemGroup(system), state) { }

        public WorldData(IGroupSystem systems, State state)
            : this((IEnumerable<ISystem>)systems, state) { }

        public WorldData(IEnumerable<ISystem> systems, State state)
        {
            Systems = systems;
            State = state ?? throw new NullReferenceException(nameof(state));
            Type = systems == null ? DataType.State : DataType.SystemAndState;
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(Type);
            writer.Pack(State);
            
            if (Type == DataType.SystemAndState)
            {
                writer.Pack(Systems);
            }
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            Type = reader.ReadEnum<DataType>();
            State = reader.Unpack<State>();
            
            Systems = (Type == DataType.SystemAndState)
                ? reader.Unpack<IEnumerable<ISystem>>() 
                : null;
        }

        public enum DataType : byte
        {
            None = 0,
            SystemAndState,
            State,
        }
    }
}
