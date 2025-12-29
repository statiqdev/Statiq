using System;

namespace Statiq.Common
{
    /// <summary>
    /// Indicates that a document property should have a different metadata name than the property name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyMetadataAttribute : Attribute
    {
        public string Name { get; }

        /// <summary>
        /// Specifies an alternate name for this property in document metadata.
        /// </summary>
        /// <param name="name">The alternate metadata name, or <c>null</c> to exclude from metadata.</param>
        public PropertyMetadataAttribute(string name)
        {
            Name = name;
        }
    }
}
