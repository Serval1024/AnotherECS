using AnotherECS.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal class Histories : IDisposable, ISerializeConstructor, IStateBindExternalInternal
    {
        public uint CurrentTick
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _tickProvider.Tick;
        }

        public TickProvider TickProvider
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _tickProvider;
        }

        private readonly List<IHistory> _children = new();
        private readonly List<IRevert> _revertChildren = new();
        private readonly TickProvider _tickProvider;
        private int _recordTickLength;

        internal Histories(ref ReaderContextSerializer reader, TickProvider tickProvider)
        {
            _tickProvider = tickProvider;
            Unpack(ref reader);
        }

        public Histories(in HistoryConfig config, TickProvider tickProvider)
        {
            _tickProvider = tickProvider;
            _recordTickLength = (int)config.recordTickLength;
        }

        public void BindExternal(State state)
        {
            foreach (var child in _children)
            {
                if (child is IStateBindExternalInternal stateBindExternalInternal)
                {
                    stateBindExternalInternal.BindExternal(state);
                }
            }
        }

        public void RegisterChild(IHistory history)
        {
            _children.Add(history);
            
            if (history is IRevert revertHistory)
            {
                _revertChildren.Add(revertHistory);
            }    
        }

        public void UnregisterChild(IHistory history)
        {
            _children.Remove(history);

            if (history is IRevert revertHistory)
            {
                _revertChildren.Remove(revertHistory);
            }
        }

        public T GetChild<T>()
            where T : IHistory
            => (T)_children.Find(p => p is T);

        public T GetChild<T>(Func<T, bool> condition)
            where T : IHistory
            => (T)_children.Find(p => p is T pT && condition(pT));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickStarted()
            => ++_tickProvider.Tick;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, IAdapter[] adapters)
        {
            if (CurrentTick - tick < _recordTickLength)
            {
                if (tick < CurrentTick)
                {
                    for (int i = 0; i < _revertChildren.Count; ++i)
                    {
                        _revertChildren[i].RevertTo(tick);
                    }

                    for (int i = 0; i < adapters.Length; ++i)
                    {
                        if (adapters[i] is IRevert revert)
                        {
                            revert.RevertTo(tick);
                        }
                    }
                }

                _tickProvider.Tick = tick;
            }
            else
            {
                throw new Exceptions.ReachedLimitHistoryBufferException(_recordTickLength, (int)(CurrentTick - tick));
            }
        }

        public void Recycle()
        {
            foreach (var child in _children)
            {
                if (child is IRecycleInternal recycle)
                {
                    recycle.Recycle();
                }
            }
        }

        public void Dispose()
        {
            foreach(var child in _children)
            {
                if (child is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_recordTickLength);
            writer.Pack(_children);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _recordTickLength = reader.ReadInt32();
            
            foreach (var child in reader.Unpack<List<IHistory>>(_tickProvider))
            {
                RegisterChild(child);
            }
        }

    }
}