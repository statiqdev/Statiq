using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.App
{
    internal class ConfiguratorList<TConfigurable> : IList<IConfigurator<TConfigurable>>
        where TConfigurable : IConfigurable
    {
        private readonly List<object> _list;

        public ConfiguratorList(List<object> list)
        {
            _list = list.ThrowIfNull(nameof(list));
        }

        public IConfigurator<TConfigurable> this[int index]
        {
            get => (IConfigurator<TConfigurable>)_list[index];
            set
            {
                if (value is object)
                {
                    _list[index] = value;
                }
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(IConfigurator<TConfigurable> item)
        {
            if (item is object)
            {
                _list.Add(item);
            }
        }

        public void Clear() => _list.Clear();

        public bool Contains(IConfigurator<TConfigurable> item) => _list.Contains(item);

        public void CopyTo(IConfigurator<TConfigurable>[] array, int arrayIndex) =>
            _list.Cast<IConfigurator<TConfigurable>>().ToList().CopyTo(array, arrayIndex);

        public IEnumerator<IConfigurator<TConfigurable>> GetEnumerator() =>
            _list.Cast<IConfigurator<TConfigurable>>().GetEnumerator();

        public int IndexOf(IConfigurator<TConfigurable> item) => _list.IndexOf(item);

        public void Insert(int index, IConfigurator<TConfigurable> item)
        {
            if (item is object)
            {
                _list.Insert(index, item);
            }
        }

        public bool Remove(IConfigurator<TConfigurable> item) => _list.Remove(item);

        public void RemoveAt(int index) => _list.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
