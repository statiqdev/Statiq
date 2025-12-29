using System;

namespace Statiq.Common
{
    /// <summary>
    /// Defines a 'split by separator process' for converting a string representation of a list of values.
    /// </summary>
    /// <remarks>
    /// Originally based on code from UniversalTypeConverter by Thorsten Bruning.
    /// Licensed under MS-PL.
    /// See https://www.codeproject.com/articles/248440/universal-type-converter.
    /// </remarks>
    internal class GenericStringSplitter : IStringSplitter
    {
        private readonly string _separator;

        /// <summary>
        /// Creates a new instance of the <see cref="GenericStringSplitter">GenericStringSplitter</see> class using the <see cref="F:TB.ComponentModel.UniversalTypeConverter.DefaultStringSeperator">semicolon</see> as seperator.
        /// </summary>
        public GenericStringSplitter()
          : this(";")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GenericStringSplitter">GenericStringSplitter</see> class.
        /// </summary>
        /// <param name="separator">The separator to use for splitting.</param>
        public GenericStringSplitter(string separator)
        {
            _separator = separator.ThrowIfNull(nameof(separator));
        }

        /// <summary>
        /// Splits the given string representation of a list of values.
        /// </summary>
        /// <param name="valueList">String representation of the list to split.</param>
        /// <returns>A list of the split values.</returns>
        public string[] Split(string valueList)
        {
            return valueList.Split(
                new string[1]
                {
                    _separator
                },
                StringSplitOptions.None);
        }
    }
}