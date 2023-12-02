using System.Collections.Generic;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    public interface IFilter : IEnumerable<EntityId> { }
}