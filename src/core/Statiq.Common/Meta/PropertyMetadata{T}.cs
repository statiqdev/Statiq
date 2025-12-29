using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// Provides metadata for the properties of a given object.
    /// </summary>
    /// <typeparam name="T">The type of object to provide property metadata for.</typeparam>
    public static class PropertyMetadata<T>
    {
        private static readonly Dictionary<string, IPropertyCallAdapter> Properties = GetPropertyMetadata();

        private static Dictionary<string, IPropertyCallAdapter> GetPropertyMetadata()
        {
            Dictionary<string, IPropertyCallAdapter> propertyMetadata =
                new Dictionary<string, IPropertyCallAdapter>(StringComparer.OrdinalIgnoreCase);

            // Do a first pass for non-attribute properties
            // This ensures actual properties will get added first and take precedence over any attributes that define colliding names
            List<(PropertyMetadataAttribute, MethodInfo)> attributeProperties =
                new List<(PropertyMetadataAttribute Property, MethodInfo Getter)>();
            foreach ((PropertyInfo property, MethodInfo getter) in
                typeof(T)
                    .GetProperties()
                    .Select(x => (x, x.GetGetMethod()))
                    .Where(x => x.Item2?.GetParameters().Length == 0))
            {
                // If there's an attribute, do this in a second pass
                PropertyMetadataAttribute attribute = property.GetCustomAttribute<PropertyMetadataAttribute>();
                if (attribute is object)
                {
                    // Only add the property for later processing if the new name isn't null
                    if (!string.IsNullOrEmpty(attribute.Name))
                    {
                        attributeProperties.Add((attribute, getter));
                    }
                }
                else
                {
                    // No attribute, so add this property
                    if (!propertyMetadata.ContainsKey(property.Name))
                    {
                        propertyMetadata.Add(property.Name, GetPropertyCallAdapter(getter));
                    }
                }
            }

            // Now that all the actual property names have been added, add ones from the attribute
            foreach ((PropertyMetadataAttribute attribute, MethodInfo getter) in attributeProperties)
            {
                if (!propertyMetadata.ContainsKey(attribute.Name))
                {
                    propertyMetadata.Add(attribute.Name, GetPropertyCallAdapter(getter));
                }
            }

            return propertyMetadata;
        }

        private static IPropertyCallAdapter GetPropertyCallAdapter(MethodInfo getter)
        {
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), getter.ReturnType);
            Delegate getterDelegate = getter.CreateDelegate(delegateType);
            Type adapterType = typeof(PropertyCallAdapter<>).MakeGenericType(typeof(T), getter.ReturnType);
            return Activator.CreateInstance(adapterType, getterDelegate) as IPropertyCallAdapter;
        }

        private interface IPropertyCallAdapter
        {
            object GetValue(T instance);
        }

        private class PropertyCallAdapter<TResult> : IPropertyCallAdapter
        {
            private readonly Func<T, TResult> _getter;

            public PropertyCallAdapter(Func<T, TResult> getter)
            {
                _getter = getter;
            }

            public object GetValue(T instance) => _getter.Invoke(instance);
        }

        /// <summary>
        /// Gets an <see cref="IMetadata"/> for a given instance of the object type.
        /// </summary>
        /// <param name="instance">The object instance to present as metadata.</param>
        /// <returns>A metadata collection for the properties of the provided instance.</returns>
        public static IMetadata For(T instance) => new PropertyMetadataInstance(instance);

        private class PropertyMetadataInstance : IMetadata
        {
            private readonly T _instance;

            /// <inheritdoc/>
            public PropertyMetadataInstance(T instance)
            {
                _instance = instance.ThrowIfNull(nameof(instance));
            }

            /// <inheritdoc/>
            public bool ContainsKey(string key) => Properties.ContainsKey(key);

            /// <inheritdoc/>
            public object this[string key]
            {
                get
                {
                    key.ThrowIfNull(nameof(key));
                    if (!TryGetValue(key, out object value))
                    {
                        throw new KeyNotFoundException("The key " + key + " was not found, use Get() to provide a default value.");
                    }
                    return value;
                }
            }

            /// <inheritdoc/>
            // Enumerate the keys seperatly so we don't evaluate values
            public IEnumerable<string> Keys => Properties.Keys;

            /// <inheritdoc/>
            public IEnumerable<object> Values => Properties.Values.Select(x => x.GetValue(_instance));

            /// <inheritdoc/>
            public bool TryGetRaw(string key, out object value)
            {
                if (Properties.TryGetValue(key, out IPropertyCallAdapter adapter))
                {
                    value = adapter.GetValue(_instance);
                    return true;
                }
                value = default;
                return false;
            }

            /// <inheritdoc/>
            public bool TryGetValue(string key, out object value) => this.TryGetValue<object>(key, out value);

            /// <inheritdoc/>
            // The Select ensures LINQ optimizations won't turn this into a recursive call to Count
            public int Count => this.Select(_ => (object)null).Count();

            /// <inheritdoc/>
            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                foreach (KeyValuePair<string, IPropertyCallAdapter> item in Properties)
                {
                    object rawValue = item.Value.GetValue(_instance);
                    yield return new KeyValuePair<string, object>(item.Key, TypeHelper.ExpandValue(item.Key, rawValue, this));
                }
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc/>
            public IEnumerator<KeyValuePair<string, object>> GetRawEnumerator()
            {
                foreach (KeyValuePair<string, IPropertyCallAdapter> item in Properties)
                {
                    object rawValue = item.Value.GetValue(_instance);
                    yield return new KeyValuePair<string, object>(item.Key, rawValue);
                }
            }
        }
    }
}
