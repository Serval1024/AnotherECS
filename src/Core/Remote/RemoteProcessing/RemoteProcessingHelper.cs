﻿using AnotherECS.Serializer;

namespace AnotherECS.Core.Remote
{
    internal class RemoteProcessingHelper
    {
        public static DependencySerializer[] GetDependencySerializer(SerializationLevel level)
            => level switch
            {
                SerializationLevel.StateData => _dependencyStateSerializationLevel0Cache,
                SerializationLevel.StateDataAndConfig => _dependencyStateSerializationLevel1Cache,
                SerializationLevel.World => _dependencyStateSerializationLevel1Cache,
                _ => throw new System.NotImplementedException(),
            };

        public static readonly DependencySerializer[] _dependencyStateSerializationLevel0Cache
            = new[] { new DependencySerializer()
            { 
                id = 0, 
                value = StateSerializationLevel.Data 
            }};

        public static readonly DependencySerializer[] _dependencyStateSerializationLevel1Cache
            = new[] { new DependencySerializer()
            { 
                id = 0, 
                value = StateSerializationLevel.Data | StateSerializationLevel.Config
            }};
    }
}
