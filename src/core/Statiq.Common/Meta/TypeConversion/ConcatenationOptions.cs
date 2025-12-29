using System;

namespace Statiq.Common
{
    /// <summary>Defines Options for a string concatenation.</summary>
    /// <remarks>
    /// Originally based on code from UniversalTypeConverter by Thorsten Bruning.
    /// Licensed under MS-PL.
    /// See https://www.codeproject.com/articles/248440/universal-type-converter.
    /// </remarks>
    [Flags]
    internal enum ConcatenationOptions
    {
        /// <summary>No options are used.</summary>
        None = 0,

        /// <summary>Null values are ignored on concatenation.</summary>
        IgnoreNull = 1,

        /// <summary>Empty values are ignored on concatenation.</summary>
        IgnoreEmpty = 2,

        /// <summary>
        /// The default value for concatenations. Same as <see cref="F:TB.ComponentModel.ConcatenationOptions.None">ConcatenationOptions.None</see>.
        /// </summary>
        Default = 0,
    }
}