using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core.Remote.Local
{
    public class LocalHubProvider : IEnumerable<LocalProvider>
    {
        private readonly List<LocalProvider> _children = new();
        private double _time;

        public int ChildCount => _children.Count;

        public LocalHubProvider(int childCount)
        {
            long playerCounter = 0;

            for (int i = 0; i < childCount; i++)
            {
                Add(new LocalProvider(
                    new Player(++playerCounter, i == 0 ? ClientRole.Master : ClientRole.Client)
                    ));
            }
        }

        public void Update(double deltaTime)
        {
            _time += deltaTime;
        }

        public LocalProvider Get(int index)
            => _children[index];
       
        public void SendOther(Player sender, byte[] bytes)
        {
            for (int i = 0; i < _children.Count; ++i)
            {
                if (_children[i].Player != sender)
                {
                    _children[i].Send(bytes);
                }
            }
        }

        public void Send(Player target, byte[] bytes)
        {
            for (int i = 0; i < _children.Count; ++i)
            {
                if (_children[i].Player == target)
                {
                    _children[i].Send(bytes);
                    return;
                }
            }
        }

        public void ConnectAll()
        {
            for (int i = 0; i < _children.Count; ++i)
            {
                for (int j = 0; j < _children.Count; ++j)
                {
                    _children[i].Connect(_children[j].Player);
                }
            }
        }

        public void DisconnectAll()
        {
            for (int i = 0; i < _children.Count; ++i)
            {
                for (int j = 0; j < _children.Count; ++j)
                {
                    _children[i].Disconnect(_children[j].Player);
                }
            }

            _children.Clear();
        }

        public void Disconnect(Player player)
        {
            for (int i = 0; i < _children.Count; ++i)
            {
                _children[i].Disconnect(player);   
            }

            _children.RemoveAll(p => p.Player == player);
        }

        public double GetGlobalTime()
            => _time;

        public Player[] GetPlayers()
            => _children
            .Select(p => p.Player)
            .ToArray();

        public IEnumerator<LocalProvider> GetEnumerator()
            => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _children.GetEnumerator();


        internal void Add(LocalProvider child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            _children.Add(child);
            child.Parent = this;
        }

        internal void Remove(LocalProvider child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            _children.Remove(child);
            child.Parent = null;
        }
    }
}
