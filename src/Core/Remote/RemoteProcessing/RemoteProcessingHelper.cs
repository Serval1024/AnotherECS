using AnotherECS.Serializer;

namespace AnotherECS.Core.Remote
{
    internal class RemoteProcessingHelper
    {
        public static DependencySerializer[] GetDependencySerializer(StateSerializationLevel level)
            => level == StateSerializationLevel.Data
                ? _dependencyStateSerializationLevel0Cache
                : _dependencyStateSerializationLevel1Cache;

        public static readonly DependencySerializer[] _dependencyStateSerializationLevel0Cache
            = new[] { new DependencySerializer() { id = 0, value = StateSerializationLevel.Data } };

        public static readonly DependencySerializer[] _dependencyStateSerializationLevel1Cache
            = new[] { new DependencySerializer() { id = 0, value = StateSerializationLevel.DataAndConfig } };
    }
}
