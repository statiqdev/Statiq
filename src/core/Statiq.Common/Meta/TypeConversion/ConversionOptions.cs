using System;

namespace Statiq.Common
{
    /// <summary>Defines options for a conversion.</summary>
    /// <remarks>
    /// Originally based on code from UniversalTypeConverter by Thorsten Bruning.
    /// Licensed under MS-PL.
    /// See https://www.codeproject.com/articles/248440/universal-type-converter.
    /// </remarks>
    [Flags]
    internal enum ConversionOptions
    {
        /// <summary>No options are used.</summary>
        None = 0,

        /// <summary>
        /// Includes some typical conversions, e.g. "yes" to true, 'n' to false, true to 'T', etc.
        /// This option is used by default.
        /// </summary>
        EnhancedTypicalValues = 1,

        /// <summary>
        /// Returns the default value of the given type of destination if the given value is null and the type of destination doesn't support null.
        /// </summary>
        AllowDefaultValueIfNull = 2,

        /// <summary>
        /// Returns the default value of the given type of destination if the given value is a string containing only whitespace but no conversion from whitespace is supported.
        /// </summary>
        AllowDefaultValueIfWhitespace = 4,

        /// <summary>
        /// The default value for conversions. Same as <see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">EnhancedTypicalValues</see>.
        /// </summary>
        Default = EnhancedTypicalValues, // 0x00000001
    }
}