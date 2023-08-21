using System;
using System.Collections.Generic;

namespace AnotherECS.Converter
{
    public interface ITypeToUshort : ITypeToId<ushort> { }

    public interface ITypeToId<UId>
        where UId : unmanaged
    {
        Type IdToType(UId id);
        UId TypeToId(Type type);
        Dictionary<UId, Type> GetAssociationTable();
        int Count();
    }
}

