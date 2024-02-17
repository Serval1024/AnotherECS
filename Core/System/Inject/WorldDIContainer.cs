using AnotherECS.Core.Exceptions;
using AnotherECS.Core.Inject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Core
{
    internal readonly struct WorldDIContainer
    {        
        private readonly State _state;
        private readonly Dictionary<Type, InjectData> _table;

        private readonly Mode _mode;

        internal WorldDIContainer(State state)
            : this(state, Mode.FullReflection, null) { }

        internal WorldDIContainer(State state, Dictionary<Type, InjectData> table)
            : this(state, Mode.TableReflection, table) { }

        internal WorldDIContainer(State state, Mode mode, Dictionary<Type, InjectData> table)
        {
            _state = state;
            _table = table;
            _mode = mode;
        }

        public void Inject(IEnumerable enumerable)
        {
            switch(_mode)
            {
                case Mode.TableReflection:
                    {
                        foreach (var obj in enumerable)
                        {
                            InjectTableMode(obj);
                        }
                        break;
                    }
                case Mode.FullReflection:
                    {
                        foreach (var obj in enumerable)
                        {
                            InjectReflectionMode(obj);
                        }
                        break;
                    }
            }
        }

        public void InjectTableMode(object target)
        {
            var type = target.GetType();
            if (_table.TryGetValue(type, out var injectData))
            {
                foreach(var memberName in injectData.memberNameInjectAttributes)
                {
                    SetValue(type, memberName, target);
                }
            }
        }

        public void InjectReflectionMode(object target)
        {
            var type = target.GetType();

            foreach (var member in
                type
                .GetFieldsAndProperties(SystemReflectionUtils.MEMBER_INJECT_FLAGS)
                .Where(p => p.GetCustomAttribute<InjectAttribute>() != null)
                )
            {
                SetValue(member, target);
            }
        }

        public T Resolve<T>()
            => (T)Resolve(typeof(T));

        public object Resolve(Type type)
        {
            if (typeof(BFilter).IsAssignableFrom(type))
            {
                return ResolveFilter(type);
            }
            else if (typeof(IConfig).IsAssignableFrom(type))
            {
                return ResolveConfig(type);
            }

            return default;
        }

        private BFilter ResolveFilter(Type type)
        {
            var includeTypes = type.GetGenericArguments();

            var mask = new Mask();
            foreach (var includeType in includeTypes)
            {
                mask.AddInclude(_state.GetIdByType(includeType));
            }

            return _state.CreateFilter(type, ref mask);
        }

        private IConfig ResolveConfig(Type type)
            => _state.GetConfig(type);

        private void SetValue(Type type, string memberName, object target)
        {
            var member = type.GetFieldOrProperty(memberName, SystemReflectionUtils.MEMBER_INJECT_FLAGS);
            SetValue(member, target);
        }

        private void SetValue(MemberInfo member, object target)
        {
            var value = Resolve(member.GetMemberType()) ?? throw new InjectException(member.GetMemberType(), member.GetMemberName());
            member.SetValue(target, value);
        }


        public enum Mode
        {
            TableReflection,
            FullReflection,
        }
    }
}