using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Statiq.Common
{
    /// <summary>
    /// Defines a string representation builder for a value list.
    /// </summary>
    /// <remarks>
    /// Originally based on code from UniversalTypeConverter by Thorsten Bruning.
    /// Licensed under MS-PL.
    /// See https://www.codeproject.com/articles/248440/universal-type-converter.
    /// </remarks>
    internal class GenericStringConcatenator : IStringConcatenator
    {
        private readonly string _seperator;
        private readonly string _nullValue;
        private readonly ConcatenationOptions _concatenationOptions;

        /// <summary>
        /// Creates a new instance of the <see cref="GenericStringConcatenator">GenericStringConcatenator</see> class.<br></br>
        /// Using the <see cref="UniversalTypeConverter.DefaultStringSeperator">semicolon</see> as seperator.<br></br>
        /// Using <see cref="UniversalTypeConverter.DefaultNullStringValue">".null."</see> for null values.<br></br>
        /// Using <see cref="ConcatenationOptions.Default">ConcatenationOptions.Default</see>.
        /// </summary>
        public GenericStringConcatenator()
          : this(";")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GenericStringConcatenator">GenericStringConcatenator</see> class.<br></br>
        /// Using the the given seperator.<br></br>
        /// Using <see cref="UniversalTypeConverter.DefaultNullStringValue">".null."</see> for null values.<br></br>
        /// Using <see cref="ConcatenationOptions.Default">ConcatenationOptions.Default</see>.
        /// </summary>
        /// <param name="seperator">Seperator.</param>
        public GenericStringConcatenator(string seperator)
          : this(seperator, ConcatenationOptions.None)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GenericStringConcatenator">GenericStringConcatenator</see> class.<br></br>
        /// Using the the given seperator.<br></br>
        /// Using <see cref="UniversalTypeConverter.DefaultNullStringValue">".null."</see> for null values.<br></br>
        /// Using the given options.
        /// </summary>
        /// <param name="seperator">Seperator.</param>
        /// <param name="concatenationOptions">Options.</param>
        public GenericStringConcatenator(string seperator, ConcatenationOptions concatenationOptions)
          : this(seperator, ".null.", concatenationOptions)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GenericStringConcatenator">GenericStringConcatenator</see> class.<br></br>
        /// Using the the given seperator.<br></br>
        /// Using the given null value.<br></br>
        /// Using <see cref="ConcatenationOptions.Default">ConcatenationOptions.Default</see>.
        /// </summary>
        /// <param name="seperator">Seperator.</param>
        /// <param name="nullValue">Null value.</param>
        public GenericStringConcatenator(string seperator, string nullValue)
          : this(seperator, nullValue, ConcatenationOptions.None)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GenericStringConcatenator">GenericStringConcatenator</see> class using the given settings.
        /// </summary>
        /// <param name="seperator">Seperator.</param>
        /// <param name="nullValue">Null value.</param>
        /// <param name="concatenationOptions">Options.</param>
        public GenericStringConcatenator(string seperator, string nullValue, ConcatenationOptions concatenationOptions)
        {
            _seperator = seperator.ThrowIfNull(nameof(seperator));
            _nullValue = nullValue;
            _concatenationOptions = concatenationOptions;
        }

        /// <summary>Concatenates the given values to a string.</summary>
        /// <param name="values">Values to concatenate.</param>
        /// <returns>String representation.</returns>
        public string Concatenate(string[] values)
        {
            return ConcatenateCore(((IEnumerable<string>)values).Where<string>((Func<string, bool>)(v => !IgnoreValue(v))).Select<string, string>((Func<string, string>)(value => value ?? _nullValue)).ToArray<string>());
        }

        private bool IgnoreValue(string value)
        {
            return (value is null && (_concatenationOptions & ConcatenationOptions.IgnoreNull) == ConcatenationOptions.IgnoreNull) || (value == string.Empty && (_concatenationOptions & ConcatenationOptions.IgnoreEmpty) == ConcatenationOptions.IgnoreEmpty);
        }

        /// <summary>
        /// Concatenates the given values to a string.<br></br>
        /// This is the core routine to override within subclasses.
        /// </summary>
        /// <param name="values">Values to concatenate.</param>
        /// <returns>String representation.</returns>
        protected virtual string ConcatenateCore(string[] values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str in values)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(_seperator);
                }

                stringBuilder.Append(str);
            }
            return stringBuilder.ToString();
        }
    }
}