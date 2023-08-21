using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AnotherECS.Core
{
    public class MTWorld : IWorld
    {
        private readonly IWorld _world;
        private Queue<ITickEvent> _events;
        private readonly ThreadController<Command> _thread;

        public MTWorld(IWorld world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _thread = new(__THREAD_handler);
        }

        public void Init()
        {
            _thread.SendCommand(new Command(Command.Type.Init));
        }

        public void Tick(uint tickCount)
        {
            if (tickCount != 0)
            {
                _thread.SendCommand(new Command(Command.Type.Tick, tickCount));
            }
        }

        public void Destroy()
        {
            _thread.SendCommand(new Command(Command.Type.Destroy));
        }

        public void Dispose()
        {
            _thread.SendCommand(new Command(Command.Type.Dispose));
        }

        public void Send(BaseEvent @event)
        {
            //_events.Enqueue();
        }




        private void __THREAD_handler(object thread)
        {
            __THREAD_handler(thread as ThreadController<Command>);
        }

        private void __THREAD_handler(ThreadController<Command> thread)
        {
            while (true)
            {
                thread.Wait();

                if (thread.IsShutdown)
                {
                    return;
                }

                while(thread.Tasks.TryDequeue(out var command))
                {
                    __THREAD_handler(ref command);
                }

                if (thread.IsShutdown)
                {
                    return;
                }
            }
        }

        private void __THREAD_handler(ref Command command)
        {
            switch (command.type)
            {
                case Command.Type.Init:
                    _world.Init();
                    break;

                case Command.Type.Tick:
                    _world.Tick(command.tick);
                    break;

                case Command.Type.Destroy:
                    _world.Destroy();
                    break;

                case Command.Type.Dispose:
                    _thread.Shutdown();
                    _world.Dispose();
                    break;
            }
        }


        private readonly struct Command
        {
            public readonly Type type;
            public readonly uint tick;

            public Command(Type type)
            {
                this.type = type;
                this.tick = 0;
            }

            public Command(Type type, uint tick)
            {
                this.type = type;
                this.tick = tick;
            }

            public enum Type
            {
                Init,
                Tick,
                Destroy,
                Dispose,
            }
        }

        private class ThreadController<T>
        {
            private readonly Thread _thread;
            private readonly AutoResetEvent _waitHandle;

            private readonly ConcurrentQueue<T> _tasks = new();

            volatile private bool _isBusy;
            volatile private bool _isShutdown;

            public bool IsBusy
                => _isBusy;

            public bool IsShutdown
                => _isShutdown;

            public ConcurrentQueue<T> Tasks
                => _tasks;
            

            public ThreadController(ParameterizedThreadStart function)
            {
                _thread = new Thread(function);
            }

            public void SendCommand(T command)
            {
                _tasks.Enqueue(command);
                Pulse();
            }

            public void Wait()
            {
                _isBusy = false;
                _waitHandle.WaitOne();
            }

            public void Pulse()
            {
                _isBusy = true;
                _waitHandle.Set();
            }

            public void Shutdown()
            {
                _isShutdown = true;
                Pulse();
            }
        }
    }
}