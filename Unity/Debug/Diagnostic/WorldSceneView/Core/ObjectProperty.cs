using AnotherECS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReflectionUtils = AnotherECS.Debug.Diagnostic.UIElements.ReflectionUtils;

namespace AnotherECS.Unity.Debug.Diagnostic
{
    public struct ObjectProperty
    {
        private object _root;
        private readonly object _target;
        private readonly Type _targetType;
        private readonly string _path;
        private readonly string _name;
        private readonly Dictionary<Type, object> _userData;

        public object Root => _root;
        public string Path => _path;

        public ObjectProperty(object target, HashSet<object> userData = null)
            : this(target, target.GetType(), userData) { }

        public ObjectProperty(object target, Type type, HashSet<object> userData = null)
        {
            _userData = userData?.ToDictionary(k => k.GetType(), v => v);
            _root = target ?? throw new ArgumentNullException(nameof(target));
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _targetType = type ?? throw new ArgumentNullException(nameof(type));
            _path = string.Empty;
            _name = type.Name;
        }

        public ObjectProperty(object root, string path, HashSet<object> userData = null)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _target = default;
            _name = default;
            _userData = userData?.ToDictionary(k => k.GetType(), v => v);

            var iterator = new PathIterator(path);
            var target = (_root, _root.GetType());
            while(!iterator.IsEnd())
            {
                target = Get(ref iterator, target);
                _name = iterator.GetName();
                iterator = iterator.Next();

                if (target.Item2 == null)
                {
                    throw new InvalidOperationException();
                }
            }
            _target = target.Item1;
            _targetType = target.Item2;
        }

        private ObjectProperty(object root, object target, Type type, string path, string name, Dictionary<Type, object> userData = null)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _target = target;
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _targetType = type ?? throw new ArgumentNullException(nameof(type));
            _userData = userData;
        }

        public ObjectProperty GetPrivateChild(string name)
            => GetChildInternal(name, ReflectionUtils.instanceFlags);
                
        public ObjectProperty GetRoot()
            => new(_root, _root, _root.GetType(), string.Empty, _root.GetType().Name, _userData);

        public ObjectProperty GetChild(string name)
            => GetChildInternal(name, ReflectionUtils.publicFlags);

        private ObjectProperty GetChildInternal(string name, System.Reflection.BindingFlags bindingFlags)
        {
            var field = _targetType.GetFieldOrProperty(name, bindingFlags);

            return new(_root,
                _target != null ? field.GetValue(_target) : null,
                field.GetMemberType(),
                PathCombine(_path, name),
                name,
                _userData
                );
        }

        public int ChildCount()
        {
            if (typeof(IEnumerable).IsAssignableFrom(_target.GetType()))
            {
                if (_target == null)
                {
                    return 0;
                }

                return (_target as IEnumerable)
                    .Cast<object>()
                    .Count();
            }
            else
            {
                return _targetType
                    .GetType()
                    .GetFieldsAndProperties(ReflectionUtils.publicFlags)
                    .Count();
            }
        }

        public IEnumerable<ObjectProperty> GetChildren()
        {
            var root = _root;
            var path = _path;
            var userData = _userData;
            if (typeof(IEnumerable).IsAssignableFrom(_targetType))
            {
                if (_target == null)
                {
                    return Enumerable.Empty<ObjectProperty>();
                }

                var iEnumerable = _target as IEnumerable;
                return iEnumerable
                    .Cast<object>()
                    .Select((p, i) => new ObjectProperty(
                        root,
                        p,
                        TryGetType(p, iEnumerable),
                        PathCombine(path, i.ToString()),
                        $"[{i}] {p.GetType().Name}",
                        userData
                        )
                    );
            }
            else
            {
                var target = _target;
                return _targetType
                    .GetFieldsAndProperties(ReflectionUtils.publicFlags)
                    .Select(p => new ObjectProperty(
                        root,
                        target != null ? p.GetValue(target) : null,
                        p.GetMemberType(),
                        PathCombine(path, p.GetMemberName()),
                        p.GetMemberName(),
                        userData
                        )
                    );
            }
        }

        public bool IsValueType()
            => _targetType.IsValueType;

        public object GetValue()
            => _target;

        public void SetValue(object value)
        {
            var iterator = new PathIterator(_path);
            FindAndSet(ref _root, ref iterator, value);
        }


        public T GetValue<T>()
            => (T)_target;

        public bool IsValueNull()
            => _target == null;

        public Type GetFieldType()
            => _targetType;

        public object GetFieldValue()
            => _target;

        public string GetFieldDisplayName()
            => _name;

        public ObjectProperty ToFieldDisplayName(string value)
            => new(_root, _target, _targetType, _path, value, _userData);

        public PathIterator GetPathIterator()
            => new(_path);

        public T GetUserData<T>()
            => _userData != null && _userData.TryGetValue(typeof(T), out var value) 
            ? (T)value 
            : default;

        private static (object, Type) Get(ref PathIterator iterator, object target)
        {
            if (iterator.IsIndex())
            {
                if (target is IEnumerable iEnumerable)
                {
                    int index = iterator.GetIndex();
                    foreach (var e in iEnumerable)
                    {
                        if (index == 0)
                        {
                            return (e, TryGetType(e, iEnumerable));
                        }
                        --index;
                    }
                }
            }
            else
            {
                var field = target.GetType().GetFieldOrProperty(iterator.GetName(), ReflectionUtils.instanceFlags);
                return (field.GetValue(target), field.GetMemberType());
            }

            return default;
        }

        private static Type TryGetType(object target, IEnumerable iEnumerable)
            => (target != null) 
                ? target.GetType()
                : ExtractTypeFromIEnumerable(iEnumerable);
            
        private static Type ExtractTypeFromIEnumerable(IEnumerable iEnumerable)
        {
            var interfaceEnumerableGeneric = iEnumerable.GetType().GetInterface($"{typeof(IEnumerable).Name}`1");
            if (interfaceEnumerableGeneric != null)
            {
                return interfaceEnumerableGeneric.GenericTypeArguments[0];
            }
            return null;
        }

        private static string PathCombine(string path0, string path1)
            => string.IsNullOrEmpty(path0)
                ? path1
                : path0 + PathIterator.PathSeparate + path1;

        private static void FindAndSet(ref object data, ref PathIterator iterator, object value)
        {
            var target = data;
            data = Set(ref iterator, target, value);
        }

        private static object Set(ref PathIterator iterator, object target, object value)
        {
            if (iterator.IsIndex())
            {
                if (target is Collections.ICollection iCollection)
                {
                    uint index = (uint)iterator.GetIndex();
                    iterator = iterator.Next();
                    if (iterator.IsEnd())
                    {
                        iCollection.Set(index, value);
                        return iCollection;
                    }
                    else
                    {
                        var newValue = Set(ref iterator, iCollection.Get(index), value);
                        iCollection.Set(index, newValue);
                        return newValue;
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                var property = target.GetType().GetFieldOrProperty(iterator.GetName(), ReflectionUtils.instanceFlags);
                iterator = iterator.Next();
                if (iterator.IsEnd())
                {
                    property.SetValue(target, value);
                    return target;
                }
                else
                {
                    var newValue = Set(ref iterator, property.GetValue(target), value);
                    property.SetValue(target, newValue);
                    return target;
                }
            }
        }


        public struct PathIterator
        {
            public const char PathSeparate = '/';

            private int _current;
            private string[] _path;

            public PathIterator(string path)
            {
                _path = path.Split(PathSeparate, StringSplitOptions.RemoveEmptyEntries);
                _current = 0;
            }

            public PathIterator Next()
                => new() { _current = _current + 1, _path = _path };

            public string ToPath()
            {
                var builder = new StringBuilder();
                int index = _current;
                while(true)
                {
                    if (index < _path.Length - 1)
                    {
                        builder.Append(_path[index]);
                        builder.Append(PathSeparate);
                    }
                    else if (index < _path.Length)
                    {
                        builder.Append(_path[index]);
                    }
                    else
                    {
                        break;
                    }
                    ++index;
                }
                return builder.ToString();
            }

            public bool IsEnd()
                => _current >= _path.Length;

            public string GetName()
                => !IsEnd()
                ? _path[_current]
                : null;

            public bool IsIndex()
                => !IsEnd() && int.TryParse(_path[_current], out var _);

            public int GetIndex()
                => (!IsEnd() && int.TryParse(_path[_current], out var result))
                ? result
                : -1;
        }
    }
}


