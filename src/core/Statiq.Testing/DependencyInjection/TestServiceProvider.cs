using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Testing
{
    public class TestServiceProvider : IServiceProvider, IServiceCollection
    {
        private readonly IServiceCollection _collection = new ServiceCollection();
        private IServiceProvider _services;

        public TestServiceProvider(Action<IServiceCollection> configure = null)
        {
            _collection.AddSingleton<IServiceScopeFactory>(new TestServiceScopeFactory(this));
            configure?.Invoke(_collection);
            _services = _collection.BuildServiceProvider();
        }

        public object GetService(Type serviceType) => _services.GetService(serviceType);

        // IServiceCollection

        public ServiceDescriptor this[int index]
        {
            get => _collection[index];
            set
            {
                _collection[index] = value;
                _services = _collection.BuildServiceProvider();
            }
        }

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        public void Add(ServiceDescriptor item)
        {
            _collection.Add(item);
            _services = _collection.BuildServiceProvider();
        }

        public void Clear()
        {
            _collection.Clear();
            _services = _collection.BuildServiceProvider();
        }

        public bool Contains(ServiceDescriptor item) => _collection.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

        public IEnumerator<ServiceDescriptor> GetEnumerator() => _collection.GetEnumerator();

        public int IndexOf(ServiceDescriptor item) => _collection.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item)
        {
            _collection.Insert(index, item);
            _services = _collection.BuildServiceProvider();
        }

        public bool Remove(ServiceDescriptor item)
        {
            bool result = _collection.Remove(item);
            if (result)
            {
                _services = _collection.BuildServiceProvider();
            }
            return result;
        }

        public void RemoveAt(int index)
        {
            _collection.RemoveAt(index);
            _services = _collection.BuildServiceProvider();
        }

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
    }
}
