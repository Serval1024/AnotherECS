using AnotherECS.Core.Exceptions;
using AnotherECS.Core.Remote.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AnotherECS.Core.Remote
{
    public class HubBytesProvider : IEnumerable<ChildHubProvider>, IHubBytesProvider, IErrorHandler
    {
        private readonly List<ChildHubProvider> _children = new();
        private readonly IRemoteBehaviorStrategy _remoteBehaviorStrategy;

        public HubBytesProvider() { }

        public HubBytesProvider(IEnumerable<ChildHubProvider> children)
            : this(children, new LogAndThrowBehaviorStrategy()) { }

        public HubBytesProvider(IEnumerable<ChildHubProvider> children, IRemoteBehaviorStrategy behaviorStrategy)
        {
            _remoteBehaviorStrategy = behaviorStrategy;

            foreach (var child in children)
            {
                Add(child);
            }
        }

        public void Add(ChildHubProvider child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            _children.Add(child);
            child.Parent = this;
        }

        public void Remove(ChildHubProvider child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            _children.Remove(child);
            child.Parent = null;
        }

        public ChildHubProvider Get(uint worldId)
        {
            for (int i = 0; i < _children.Count; ++i)
            {
                if (_children[i].WorldId == worldId)
                {
                    return _children[i];
                }
            }
            throw new ArgumentException($"{nameof(worldId)} is not found: '{worldId}'.");
        }

        public void SendOther(uint sender, byte[] bytes)
        {
            Get(sender).Send(bytes);
        }

        public void Error(ErrorReport error)
        {
            if (_remoteBehaviorStrategy != null)
            {
                if (error.Is<UnpackCorruptedDataException>())
                {
                    var context = new BehaviorContext();
                    _remoteBehaviorStrategy.OnReceiveCorruptedData(ref context);
                }
                else if (error.Is<HistoryRevertTickLimitException>())
                {
                    var context = new BehaviorContext();
                    _remoteBehaviorStrategy.OnRevertFailed(ref context);
                }
            }
        }

        public IEnumerator<ChildHubProvider> GetEnumerator()
            => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _children.GetEnumerator();
    }
}
