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
    internal interface IStringConcatenator
    {
        /// <summary>Concatenates the given values to a string.</summary>
        /// <param name="values">Values to concatenate.</param>
        /// <returns>String representation.</returns>
        string Concatenate(string[] values);
    }
}