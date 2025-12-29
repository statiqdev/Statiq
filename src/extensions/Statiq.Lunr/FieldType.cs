using System;

namespace Statiq.Lunr
{
    [Flags]
    public enum FieldType
    {
        /// <summary>
        /// The field is searchable.
        /// </summary>
        Searchable = 1,

        /// <summary>
        /// The field value will be available on the client.
        /// </summary>
        Result = 2
    }
}
