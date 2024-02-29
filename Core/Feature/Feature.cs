using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core
{
    public abstract class Feature : IFeature
    {
        private List<IConfig> _configs;

        public Feature AddConfig<TConfig>(TConfig config)
            where TConfig : IConfig
        {
            _configs ??= new List<IConfig>();
            _configs.Add(config);
            return this;
        }

        public Feature AddConfig(params IConfig[] configs)
        {
            _configs ??= new List<IConfig>();
            foreach (var config in configs)
            {
                _configs.Add(config);
            }
            return this;
        }

        public void Install(ref Core.InstallContext context)
        {
            var childContext = new InstallContext(context);
            OnInstall(ref childContext);
            Apply(ref childContext);
            childContext.Apply(ref context);
        }

        private void Apply(ref InstallContext context)
        {
            if (_configs != null)
            {
                foreach (var config in _configs)
                {
                    context.RemoveRequestConfig(config.GetType());
                    context.AddConfig(config);
                }
            }

            Validate(ref context);
        }

        private void Validate(ref InstallContext context)
        {
            if (context.RequestConfigCount() != 0)
            {
                throw new Exceptions.FeatureRequestConfigException(context.GetRequestConfigs());
            }
        }

        public abstract void OnInstall(ref InstallContext context);


        public struct InstallContext
        {
            private HashSet<Type> _requestConfigs;
            private Core.InstallContext _parent;

            internal InstallContext(Core.InstallContext parent)
            {
                _requestConfigs = null;
                _parent = parent;
            }

            public World World => _parent.World;

            public SortOrder SystemSortOrder
            {
                get => _parent.SystemSortOrder;
                set => _parent.SystemSortOrder = value;
            }

            public void AddSystem(ISystem system)
            {
                _parent.AddSystem(system);
            }

            public void RequestConfig<T>()
                where T : IConfig
            {
                _requestConfigs ??= new HashSet<Type>();
                _requestConfigs.Add(typeof(T));
            }

            public void AddConfig<T>(T config)
                where T : IConfig
            {
                _parent.AddConfig(config);
            }

            public void AddConfig(IConfig config)
            {
                _parent.AddConfig(config);
            }

            public void AddSingle<T>(T single)
                where T : unmanaged, ISingle
            {
                _parent.AddSingle(single);
            }


            internal Type[] GetRequestConfigs()
                => _requestConfigs != null ? _requestConfigs.ToArray() : Array.Empty<Type>();

            internal int RequestConfigCount()
                => _requestConfigs != null ? _requestConfigs.Count : 0;

            internal void RemoveRequestConfig(Type type)
            {
                _requestConfigs?.Remove(type);
            }

            internal void Apply(ref Core.InstallContext parent)
            {
                parent = _parent;
            }
        }
    }
}
