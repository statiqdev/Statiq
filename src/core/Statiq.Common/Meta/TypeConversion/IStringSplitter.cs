namespace Statiq.Common
{
    /// <summary>
    /// Defines a splitter for converting a string represenation of a list of values.
    /// </summary>
    /// <remarks>
    /// Originally based on code from UniversalTypeConverter by Thorsten Bruning.
    /// Licensed under MS-PL.
    /// See https://www.codeproject.com/articles/248440/universal-type-converter.
    /// </remarks>
    internal interface IStringSplitter
    {
        /// <summary>
        /// Splits the given string represenation of a list of values.
        /// </summary>
        /// <param name="valueList">String represenation of the list to split.</param>
        /// <returns>A list of the splitted values.</returns>
        string[] Split(string valueList);
    }
}