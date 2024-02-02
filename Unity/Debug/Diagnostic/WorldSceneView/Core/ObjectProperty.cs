using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using AnotherECS.Core;
using ReflectionUtils = AnotherECS.Debug.Diagnostic.Editor.UIElements.ReflectionUtils;
using static UnityEngine.GraphicsBuffer;


namespace AnotherECS.Unity.Debug.Diagnostic
{
    public struct ObjectProperty
    {
        private object _root;
        private readonly object _target;
        private readonly string _path;
        private readonly string _name;

        public object Root => _root;
        public string Path => _path;

        public ObjectProperty(object target)
        {
            _root = target;
            _target = target;
            _path = string.Empty;
            _name = _target.GetType().Name;
        }

        public ObjectProperty(object root, string path)
        {
            _root = root;
            _path = path;
            _target = default;
            _name = default;

            var iterator = new PathIterator(path);
            var target = _root;
            while(!iterator.IsEnd())
            {
                target = Get(ref iterator, target);
                _name = iterator.GetName();
                iterator = iterator.Next();

                if (target == null)
                {
                    throw new InvalidOperationException();
                }
            }
            _target = target;
        }

        private ObjectProperty(object root, object target, string path, string name)
        {
            _root = root;
            _target = target;
            _path = path;
            _name = name;
        }

        public ObjectProperty GetPrivateChild(string name)
            => new(_root,
                _target.GetType().GetFieldOrProperty(name, ReflectionUtils.instanceFlags).GetValue(_target),
                PathCombine(_path, name),
                name);

        public ObjectProperty GetRoot()
            => new (_root, _root, string.Empty, _root.GetType().Name);

        public ObjectProperty GetChild(string name)
            => new(_root,
                _target.GetType().GetFieldOrProperty(name, ReflectionUtils.publicFlags).GetValue(_target),
                PathCombine(_path, name),
                name);

        public IEnumerable<ObjectProperty> GetChildren()
        {
            var root = _root;
            var path = _path;
            if (typeof(IEnumerable).IsAssignableFrom(_target.GetType()))
            {
                return (_target as IEnumerable)
                    .Cast<object>()
                    .Select((p, i) => new ObjectProperty(
                        root,
                        p,
                        PathCombine(path, i.ToString()),
                        $"[{i}] {p.GetType().Name}"
                        ));
            }
            else
            {
                var target = _target;
                return target
                    .GetType()
                    .GetFieldsAndProperties(ReflectionUtils.publicFlags)
                    .Select(p => new ObjectProperty(root, p.GetValue(target), PathCombine(path, p.GetMemberName()), p.GetMemberName()));
            }
        }

        public object GetValue()
            => _target;

        public void SetValue(object value)
        {
            var iterator = new PathIterator(_path);
            FindAndSet(ref _root, ref iterator, value);
        }


        public T GetValue<T>()
            => (T)_target;

        public Type GetFieldType()
            => _target.GetType();

        public object GetFieldValue()
            => _target;

        public string GetFieldDisplayName()
            => _name;

        public ObjectProperty ToFieldDisplayName(string value)
            => new(_root, _target, _path, value);

        public PathIterator GetPathIterator()
            => new(_path);

        private static object Get(ref PathIterator iterator, object target)
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
                            return e;
                        }
                        --index;
                    }
                }
            }
            else
            {
                return target.GetType().GetFieldOrProperty(iterator.GetName(), ReflectionUtils.instanceFlags).GetValue(target);
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


