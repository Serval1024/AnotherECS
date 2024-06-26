﻿using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    internal struct TickProvider : ISerialize
    {
        public uint tick;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(tick);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            tick = reader.ReadUInt32();
        }
    }
}

