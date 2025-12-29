using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Statiq.Common
{
    /// <summary>
    /// Converts a data type to another data type.
    /// </summary>
    /// <remarks>
    /// Originally based on code from UniversalTypeConverter by Thorsten Bruning.
    /// Licensed under MS-PL.
    /// See https://www.codeproject.com/articles/248440/universal-type-converter.
    /// </remarks>
    internal class UniversalTypeConverter
    {
        /// <summary>
        /// Defines the default culture which is used during conversion.
        /// Same as CultureInfo.CurrentCulture.
        /// </summary>
        public static readonly CultureInfo DefaultCulture = CultureInfo.CurrentCulture;
        private const string ImplicitOperatorMethodName = "op_Implicit";
        private const string ExplicitOperatorMethodName = "op_Explicit";

        /// <summary>
        /// Defines ".null." as the default null value which is used on string conversions.
        /// </summary>
        public const string DefaultNullStringValue = ".null.";

        /// <summary>
        /// Defines the semicolon (;) as the default seperator which is used on enumerable conversions.
        /// </summary>
        public const string DefaultStringSeperator = ";";

        private static bool TryConvertFromNull(TypeInfo destinationType, out object result, ConversionOptions options)
        {
            result = GetDefaultValueOfType(destinationType);
            if (result is null)
            {
                return true;
            }

            return (options & ConversionOptions.AllowDefaultValueIfNull) == ConversionOptions.AllowDefaultValueIfNull;
        }

        private static bool TryConvertCore(object value, Type destinationType, ref object result, CultureInfo culture, ConversionOptions options)
        {
            if (value.GetType() == destinationType)
            {
                result = value;
                return true;
            }
            if (TryConvertByDefaultTypeConverters(value, destinationType, culture, ref result))
            {
                return true;
            }

            TypeInfo typeInfo = destinationType.GetTypeInfo();
            if (TryConvertByIConvertibleImplementation(value, destinationType, culture, ref result)
                || TryConvertWithConversionOperator(value, typeInfo, ExplicitOperatorMethodName, ref result)
                || (TryConvertWithConversionOperator(value, typeInfo, ImplicitOperatorMethodName, ref result)
                || TryConvertByIntermediateConversion(value, destinationType, ref result, culture, options))
                || ((typeInfo.IsEnum && TryConvertToEnum(value, destinationType, ref result))
                || ((options & ConversionOptions.EnhancedTypicalValues) == ConversionOptions.EnhancedTypicalValues && TryConvertSpecialValues(value, destinationType, ref result))))
            {
                return true;
            }

            if ((options & ConversionOptions.AllowDefaultValueIfWhitespace) != ConversionOptions.AllowDefaultValueIfWhitespace || !(value is string) || !IsWhiteSpace((string)value))
            {
                return false;
            }

            result = GetDefaultValueOfType(typeInfo);
            return true;
        }

        private static bool TryConvertByDefaultTypeConverters(object value, Type destinationType, CultureInfo culture, ref object result)
        {
            TypeConverter converter1 = TypeDescriptor.GetConverter(destinationType);
            if (converter1 is object)
            {
                if (converter1.CanConvertFrom(value.GetType()))
                {
                    try
                    {
                        result = converter1.ConvertFrom(null, culture, value);
                        return true;
                    }
                    catch
                    {
                    }
                }
            }
            TypeConverter converter2 = TypeDescriptor.GetConverter(value.GetType());
            if (converter2 is object)
            {
                if (converter2.CanConvertTo(destinationType))
                {
                    try
                    {
                        result = converter2.ConvertTo(null, culture, value, destinationType);
                        return true;
                    }
                    catch
                    {
                    }
                }
            }
            return false;
        }

        private static bool TryConvertByIConvertibleImplementation(object value, Type destinationType, IFormatProvider formatProvider, ref object result)
        {
            IConvertible convertible = value as IConvertible;
            if (convertible is object)
            {
                try
                {
                    if (destinationType == typeof(bool))
                    {
                        result = convertible.ToBoolean(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(byte))
                    {
                        result = convertible.ToByte(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(char))
                    {
                        result = convertible.ToChar(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(DateTime))
                    {
                        result = convertible.ToDateTime(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(decimal))
                    {
                        result = convertible.ToDecimal(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(double))
                    {
                        result = convertible.ToDouble(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(short))
                    {
                        result = convertible.ToInt16(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(int))
                    {
                        result = convertible.ToInt32(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(long))
                    {
                        result = convertible.ToInt64(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(sbyte))
                    {
                        result = convertible.ToSByte(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(float))
                    {
                        result = convertible.ToSingle(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(ushort))
                    {
                        result = convertible.ToUInt16(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(uint))
                    {
                        result = convertible.ToUInt32(formatProvider);
                        return true;
                    }
                    if (destinationType == typeof(ulong))
                    {
                        result = convertible.ToUInt64(formatProvider);
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        private static bool TryConvertWithConversionOperator(object value, TypeInfo destinationType, string operatorMethodName, ref object result) =>
            TryConvertWithConversionOperator(value, value.GetType().GetTypeInfo(), destinationType, operatorMethodName, ref result)
                || TryConvertWithConversionOperator(value, destinationType, destinationType, operatorMethodName, ref result);

        private static bool TryConvertWithConversionOperator(object value, TypeInfo invokerType, TypeInfo destinationType, string operatorMethodName, ref object result)
        {
            foreach (MethodInfo methodInfo in invokerType.DeclaredMethods.Where(method =>
            {
                if (method.IsPublic)
                {
                    return method.IsStatic;
                }

                return false;
            }).Where(m => m.Name == operatorMethodName))
            {
                if (destinationType.IsAssignableFrom(methodInfo.ReturnType.GetTypeInfo()))
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == value.GetType())
                    {
                        try
                        {
                            result = methodInfo.Invoke(null, new object[1] { value });
                            return true;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return false;
        }

        private static bool TryConvertByIntermediateConversion(object value, Type destinationType, ref object result, CultureInfo culture, ConversionOptions options)
        {
            if (value is char && (destinationType == typeof(double) || destinationType == typeof(float)))
            {
                return TryConvertCore(System.Convert.ToInt16(value), destinationType, ref result, culture, options);
            }

            if ((value is double || value is float) && destinationType == typeof(char))
            {
                return TryConvertCore(System.Convert.ToInt16(value), destinationType, ref result, culture, options);
            }

            return false;
        }

        private static bool TryConvertToEnum(object value, Type destinationType, ref object result)
        {
            try
            {
                result = Enum.ToObject(destinationType, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryConvertSpecialValues(object value, Type destinationType, ref object result)
        {
            if (value is char && destinationType == typeof(bool))
            {
                return TryConvertCharToBool((char)value, ref result);
            }

            if (value is string && destinationType == typeof(bool))
            {
                return TryConvertStringToBool((string)value, ref result);
            }

            if (value is bool && destinationType == typeof(char))
            {
                return ConvertBoolToChar((bool)value, out result);
            }

            return false;
        }

        private static bool TryConvertCharToBool(char value, ref object result)
        {
            if ("1JYT".Contains(value.ToString().ToUpper()))
            {
                result = true;
                return true;
            }
            if (!"0NF".Contains(value.ToString().ToUpper()))
            {
                return false;
            }

            result = false;
            return true;
        }

        private static bool TryConvertStringToBool(string value, ref object result)
        {
            if (new List<string>(new string[8]
            {
        "1",
        "j",
        "ja",
        "y",
        "yes",
        "true",
        "t",
        ".t."
            }).Contains(value.Trim().ToLower()))
            {
                result = true;
                return true;
            }
            if (!new List<string>(new string[7]
            {
        "0",
        "n",
        "no",
        "nein",
        "false",
        "f",
        ".f."
            }).Contains(value.Trim().ToLower()))
            {
                return false;
            }

            result = false;
            return true;
        }

        private static bool ConvertBoolToChar(bool value, out object result)
        {
            result = (char)(value ? 84 : 70);
            return true;
        }

        /// <summary>
        /// Determines whether the given value can be converted to the specified type using the current CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// </summary>
        /// <typeparam name="T">The Type to test.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <returns>true if <paramref name="value" /> can be converted to <typeparamref name="T" />; otherwise, false.</returns>
        public static bool CanConvertTo<T>(object value)
        {
            T result;
            return TryConvertTo(value, out result);
        }

        /// <summary>
        /// Determines whether the given value can be converted to the specified type using the given CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// </summary>
        /// <typeparam name="T">The Type to test.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <returns>true if <paramref name="value" /> can be converted to <typeparamref name="T" />; otherwise, false.</returns>
        public static bool CanConvertTo<T>(object value, CultureInfo culture)
        {
            T result;
            return TryConvertTo(value, out result, culture);
        }

        /// <summary>
        /// Determines whether the given value can be converted to the specified type using the current CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// </summary>
        /// <typeparam name="T">The Type to test.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>true if <paramref name="value" /> can be converted to <typeparamref name="T" />; otherwise, false.</returns>
        public static bool CanConvertTo<T>(object value, ConversionOptions options)
        {
            T result;
            return TryConvertTo(value, out result, options);
        }

        /// <summary>
        /// Determines whether the given value can be converted to the specified type using the given CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// </summary>
        /// <typeparam name="T">The Type to test.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>true if <paramref name="value" /> can be converted to <typeparamref name="T" />; otherwise, false.</returns>
        public static bool CanConvertTo<T>(object value, CultureInfo culture, ConversionOptions options)
        {
            T result;
            return TryConvertTo(value, out result, culture, options);
        }

        /// <summary>
        /// Converts the given value to the given type using the current CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type to which the given value is converted.</typeparam>
        /// <param name="value">The value which is converted.</param>
        /// <param name="result">An Object instance of type <typeparamref name="T">T</typeparamref> whose value is equivalent to the given <paramref name="value">value</paramref> if the operation succeeded.</param>
        /// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
        public static bool TryConvertTo<T>(object value, out T result)
        {
            return TryConvertTo(value, out result, DefaultCulture);
        }

        /// <summary>
        /// Converts the given value to the given type using the given CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type to which the given value is converted.</typeparam>
        /// <param name="value">The value which is converted.</param>
        /// <param name="result">An Object instance of type <typeparamref name="T">T</typeparamref> whose value is equivalent to the given <paramref name="value">value</paramref> if the operation succeeded.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
        public static bool TryConvertTo<T>(object value, out T result, CultureInfo culture)
        {
            return TryConvertTo(value, out result, culture, ConversionOptions.EnhancedTypicalValues);
        }

        /// <summary>
        /// Converts the given value to the given type using the current CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type to which the given value is converted.</typeparam>
        /// <param name="value">The value which is converted.</param>
        /// <param name="result">An Object instance of type <typeparamref name="T">T</typeparamref> whose value is equivalent to the given <paramref name="value">value</paramref> if the operation succeeded.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
        public static bool TryConvertTo<T>(object value, out T result, ConversionOptions options)
        {
            return TryConvertTo(value, out result, DefaultCulture, options);
        }

        /// <summary>
        /// Converts the given value to the given type using the given CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="T">The type to which the given value is converted.</typeparam>
        /// <param name="value">The value which is converted.</param>
        /// <param name="result">An Object instance of type <typeparamref name="T">T</typeparamref> whose value is equivalent to the given <paramref name="value">value</paramref> if the operation succeeded.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
        public static bool TryConvertTo<T>(object value, out T result, CultureInfo culture, ConversionOptions options)
        {
            object result1;
            if (TryConvert(value, typeof(T), out result1, culture, options))
            {
                result = (T)result1;
                return true;
            }
            result = default(T);
            return false;
        }

        /// <summary>
        /// Converts the given value to the given type using the current CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// </summary>
        /// <typeparam name="T">The type to which the given value is converted.</typeparam>
        /// <param name="value">The value which is converted.</param>
        /// <returns>An Object instance of type <typeparamref name="T">T</typeparamref> whose value is equivalent to the given <paramref name="value">value</paramref>.</returns>
        public static T ConvertTo<T>(object value)
        {
            return ConvertTo<T>(value, DefaultCulture);
        }

        /// <summary>
        /// Converts the given value to the given type using the given CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// </summary>
        /// <typeparam name="T">The type to which the given value is converted.</typeparam>
        /// <param name="value">The value which is converted.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <returns>An Object instance of type <typeparamref name="T">T</typeparamref> whose value is equivalent to the given <paramref name="value">value</paramref>.</returns>
        public static T ConvertTo<T>(object value, CultureInfo culture)
        {
            return ConvertTo<T>(value, culture, ConversionOptions.EnhancedTypicalValues);
        }

        /// <summary>
        /// Converts the given value to the given type using the current CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// </summary>
        /// <typeparam name="T">The type to which the given value is converted.</typeparam>
        /// <param name="value">The value which is converted.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>An Object instance of type <typeparamref name="T">T</typeparamref> whose value is equivalent to the given <paramref name="value">value</paramref>.</returns>
        public static T ConvertTo<T>(object value, ConversionOptions options)
        {
            return ConvertTo<T>(value, DefaultCulture, options);
        }

        /// <summary>
        /// Converts the given value to the given type using the given CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// </summary>
        /// <typeparam name="T">The type to which the given value is converted.</typeparam>
        /// <param name="value">The value which is converted.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>An Object instance of type <typeparamref name="T">T</typeparamref> whose value is equivalent to the given <paramref name="value">value</paramref>.</returns>
        public static T ConvertTo<T>(object value, CultureInfo culture, ConversionOptions options)
        {
            return (T)Convert(value, typeof(T), culture, options);
        }

        /// <summary>
        /// Determines whether the given value can be converted to the specified type using the current CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="destinationType">The Type to test.</param>
        /// <returns>true if <paramref name="value" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
        public static bool CanConvert(object value, Type destinationType)
        {
            object result;
            return TryConvert(value, destinationType, out result);
        }

        /// <summary>
        /// Determines whether the given value can be converted to the specified type using the given CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="destinationType">The Type to test.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <returns>true if <paramref name="value" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
        public static bool CanConvert(object value, Type destinationType, CultureInfo culture)
        {
            object result;
            return TryConvert(value, destinationType, out result, culture);
        }

        /// <summary>
        /// Determines whether the given value can be converted to the specified type using the current CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="destinationType">The Type to test.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>true if <paramref name="value" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
        public static bool CanConvert(object value, Type destinationType, ConversionOptions options)
        {
            object result;
            return TryConvert(value, destinationType, out result, options);
        }

        /// <summary>
        /// Determines whether the given value can be converted to the specified type using the given CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="destinationType">The Type to test.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>true if <paramref name="value" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
        public static bool CanConvert(object value, Type destinationType, CultureInfo culture, ConversionOptions options)
        {
            object result;
            return TryConvert(value, destinationType, out result, culture, options);
        }

        /// <summary>
        /// Converts the given value to the given type using the current CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="value">The value which is converted.</param>
        /// <param name="destinationType">The type to which the given value is converted.</param>
        /// <param name="result">An Object instance of type <paramref name="destinationType">destinationType</paramref> whose value is equivalent to the given <paramref name="value">value</paramref> if the operation succeeded.</param>
        /// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
        public static bool TryConvert(object value, Type destinationType, out object result)
        {
            return TryConvert(value, destinationType, out result, DefaultCulture);
        }

        /// <summary>
        /// Converts the given value to the given type using the given CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="value">The value which is converted.</param>
        /// <param name="destinationType">The type to which the given value is converted.</param>
        /// <param name="result">An Object instance of type <paramref name="destinationType">destinationType</paramref> whose value is equivalent to the given <paramref name="value">value</paramref> if the operation succeeded.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
        public static bool TryConvert(object value, Type destinationType, out object result, CultureInfo culture)
        {
            return TryConvert(value, destinationType, out result, culture, ConversionOptions.EnhancedTypicalValues);
        }

        /// <summary>
        /// Converts the given value to the given type using the current CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="value">The value which is converted.</param>
        /// <param name="destinationType">The type to which the given value is converted.</param>
        /// <param name="result">An Object instance of type <paramref name="destinationType">destinationType</paramref> whose value is equivalent to the given <paramref name="value">value</paramref> if the operation succeeded.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
        public static bool TryConvert(object value, Type destinationType, out object result, ConversionOptions options)
        {
            return TryConvert(value, destinationType, out result, DefaultCulture, options);
        }

        /// <summary>
        /// Converts the given value to the given type using the given CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="value">The value which is converted.</param>
        /// <param name="destinationType">The type to which the given value is converted.</param>
        /// <param name="result">An Object instance of type <paramref name="destinationType">destinationType</paramref> whose value is equivalent to the given <paramref name="value">value</paramref> if the operation succeeded.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>true if <paramref name="value" /> was converted successfully; otherwise, false.</returns>
        public static bool TryConvert(object value, Type destinationType, out object result, CultureInfo culture, ConversionOptions options)
        {
            if (destinationType == typeof(object))
            {
                result = value;
                return true;
            }
            TypeInfo typeInfo = destinationType.GetTypeInfo();
            if (ValueRepresentsNull(value))
            {
                return TryConvertFromNull(typeInfo, out result, options);
            }

            if (typeInfo.IsAssignableFrom(value.GetType().GetTypeInfo()))
            {
                result = value;
                return true;
            }
            Type destinationType1 = IsGenericNullable(typeInfo) ? GetUnderlyingType(destinationType) : destinationType;
            object result1 = null;
            if (TryConvertCore(value, destinationType1, ref result1, culture, options))
            {
                result = result1;
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Converts the given value to the given type using the current CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// </summary>
        /// <param name="value">The value which is converted.</param>
        /// <param name="destinationType">The type to which the given value is converted.</param>
        /// <returns>An Object instance of type <paramref name="destinationType">destinationType</paramref> whose value is equivalent to the given <paramref name="value">value</paramref>.</returns>
        public static object Convert(object value, Type destinationType)
        {
            return Convert(value, destinationType, DefaultCulture);
        }

        /// <summary>
        /// Converts the given value to the given type using the given CultureInfo and the <see cref="ConversionOptions">ConversionOptions</see>.<see cref="F:TB.ComponentModel.ConversionOptions.EnhancedTypicalValues">ConvertSpecialValues</see>.
        /// </summary>
        /// <param name="value">The value which is converted.</param>
        /// <param name="destinationType">The type to which the given value is converted.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <returns>An Object instance of type <paramref name="destinationType">destinationType</paramref> whose value is equivalent to the given <paramref name="value">value</paramref>.</returns>
        public static object Convert(object value, Type destinationType, CultureInfo culture)
        {
            return Convert(value, destinationType, culture, ConversionOptions.EnhancedTypicalValues);
        }

        /// <summary>
        /// Converts the given value to the given type using the current CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// </summary>
        /// <param name="value">The value which is converted.</param>
        /// <param name="destinationType">The type to which the given value is converted.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>An Object instance of type <paramref name="destinationType">destinationType</paramref> whose value is equivalent to the given <paramref name="value">value</paramref>.</returns>
        public static object Convert(object value, Type destinationType, ConversionOptions options)
        {
            return Convert(value, destinationType, DefaultCulture, options);
        }

        /// <summary>
        /// Converts the given value to the given type using the given CultureInfo and the given <see cref="ConversionOptions">ConversionOptions</see>.
        /// </summary>
        /// <param name="value">The value which is converted.</param>
        /// <param name="destinationType">The type to which the given value is converted.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <param name="options">The options which are used for conversion.</param>
        /// <returns>An Object instance of type <paramref name="destinationType">destinationType</paramref> whose value is equivalent to the given <paramref name="value">value</paramref>.</returns>
        public static object Convert(object value, Type destinationType, CultureInfo culture, ConversionOptions options)
        {
            object result;
            if (TryConvert(value, destinationType, out result, culture, options))
            {
                return result;
            }

            throw new InvalidConversionException(value, destinationType);
        }

        /// <summary>
        /// Converts all elements of the given list to the given type.
        /// The result is configurable further more before first iteration.
        /// </summary>
        /// <typeparam name="T">The type to which the given values are converted.</typeparam>
        /// <param name="values">The list of values which are converted.</param>
        /// <returns>List of converted values.</returns>
        public static EnumerableConversion<T> ConvertToEnumerable<T>(IEnumerable values)
        {
            return new EnumerableConversion<T>(values);
        }

        /// <summary>
        /// Splits the given string by using the semicolon (;) as a seperator and converts all elements of the result to the given type.
        /// The result is configurable further more before first iteration.
        /// </summary>
        /// <typeparam name="T">The type to which the given values are converted.</typeparam>
        /// <param name="valueList">The string representation of the list of values to convert.</param>
        /// <returns>List of converted values.</returns>
        public static EnumerableStringConversion<T> ConvertToEnumerable<T>(string valueList)
        {
            return ConvertToEnumerable<T>(valueList, new GenericStringSplitter());
        }

        /// <summary>
        /// Splits the given string by using the given seperator and converts all elements of the result to the given type.
        /// The result is configurable further more before first iteration.
        /// </summary>
        /// <typeparam name="T">The type to which the given values are converted.</typeparam>
        /// <param name="valueList">The string representation of the list of values to convert.</param>
        /// <param name="seperator">The value seperator which is used in <paramref name="valueList">valueList</paramref>.</param>
        /// <returns>List of converted values.</returns>
        public static EnumerableStringConversion<T> ConvertToEnumerable<T>(string valueList, string seperator)
        {
            return ConvertToEnumerable<T>(valueList, new GenericStringSplitter(seperator));
        }

        /// <summary>
        /// Splits the given string by using the given splitter and converts all elements of the result to the given type.
        /// The result is configurable further more before first iteration.
        /// </summary>
        /// <typeparam name="T">The type to which the given values are converted.</typeparam>
        /// <param name="valueList">The string representation of the list of values to convert.</param>
        /// <param name="stringSplitter">The splitter to use.</param>
        /// <returns>List of converted values.</returns>
        public static EnumerableStringConversion<T> ConvertToEnumerable<T>(string valueList, IStringSplitter stringSplitter)
        {
            return new EnumerableStringConversion<T>(valueList, stringSplitter);
        }

        /// <summary>
        /// Converts all elements of the given list to the given type.
        /// The result is configurable further more before first iteration.
        /// </summary>
        /// <param name="values">The list of values which are converted.</param>
        /// <param name="destinationType">The type to which the given values are converted.</param>
        /// <returns>List of converted values.</returns>
        public static EnumerableConversion<object> ConvertToEnumerable(IEnumerable values, Type destinationType)
        {
            return new EnumerableConversion<object>(values, destinationType);
        }

        /// <summary>
        /// Splits the given string by using the semicolon (;) as a seperator and converts all elements of the result to the given type.
        /// The result is configurable further more before first iteration.
        /// </summary>
        /// <param name="valueList">The string representation of the list of values to convert.</param>
        /// <param name="destinationType">The type to which the given values are converted.</param>
        /// <returns>List of converted values.</returns>
        public static EnumerableStringConversion<object> ConvertToEnumerable(string valueList, Type destinationType)
        {
            return ConvertToEnumerable(valueList, destinationType, ";");
        }

        /// <summary>
        /// Splits the given string by using the given seperator and converts all elements of the result to the given type.
        /// The result is configurable further more before first iteration.
        /// </summary>
        /// <param name="valueList">The string representation of the list of values to convert.</param>
        /// <param name="destinationType">The type to which the given values are converted.</param>
        /// <param name="seperator">The value seperator which is used in <paramref name="valueList">valueList</paramref>.</param>
        /// <returns>List of converted values.</returns>
        public static EnumerableStringConversion<object> ConvertToEnumerable(string valueList, Type destinationType, string seperator)
        {
            return ConvertToEnumerable(valueList, destinationType, new GenericStringSplitter(seperator));
        }

        /// <summary>
        /// Splits the given string by using the given splitter and converts all elements of the result to the given type.
        /// The result is configurable further more before first iteration.
        /// </summary>
        /// <param name="valueList">The string representation of the list of values to convert.</param>
        /// <param name="destinationType">The type to which the given values are converted.</param>
        /// <param name="stringSplitter">The splitter to use.</param>
        /// <returns>List of converted values.</returns>
        public static EnumerableStringConversion<object> ConvertToEnumerable(string valueList, Type destinationType, IStringSplitter stringSplitter)
        {
            return new EnumerableStringConversion<object>(valueList, destinationType, stringSplitter);
        }

        /// <summary>
        /// Converts the given value list to a semicolon seperated string.
        /// </summary>
        /// <param name="values">Values to convert to string.</param>
        /// <returns>String representation of the given value list.</returns>
        public static string ConvertToStringRepresentation(IEnumerable values)
        {
            return ConvertToStringRepresentation(values, DefaultCulture, new GenericStringConcatenator());
        }

        /// <summary>
        /// Converts the given value list to a string where all values a seperated by the given seperator.
        /// </summary>
        /// <param name="values">Values to convert to string.</param>
        /// <param name="seperator">Seperator.</param>
        /// <returns>String representation of the given value list.</returns>
        public static string ConvertToStringRepresentation(IEnumerable values, string seperator)
        {
            return ConvertToStringRepresentation(values, DefaultCulture, new GenericStringConcatenator(seperator));
        }

        /// <summary>
        /// Converts the given value list to a string where all values a seperated by the given seperator.
        /// </summary>
        /// <param name="values">Values to convert to string.</param>
        /// <param name="seperator">Seperator.</param>
        /// <param name="nullValue">The string which is used for null values.</param>
        /// <returns>String representation of the given value list.</returns>
        public static string ConvertToStringRepresentation(IEnumerable values, string seperator, string nullValue)
        {
            return ConvertToStringRepresentation(values, DefaultCulture, new GenericStringConcatenator(seperator, nullValue));
        }

        /// <summary>
        /// Converts the given value list to a semicolon seperated string.
        /// </summary>
        /// <param name="values">Values to convert to string.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <returns>String representation of the given value list.</returns>
        public static string ConvertToStringRepresentation(IEnumerable values, CultureInfo culture)
        {
            return ConvertToStringRepresentation(values, culture, new GenericStringConcatenator());
        }

        /// <summary>Converts the given value list to a string.</summary>
        /// <param name="values">Values to convert to string.</param>
        /// <param name="stringConcatenator">The concatenator which is used to build the string.</param>
        /// <returns>String representation of the given value list.</returns>
        public static string ConvertToStringRepresentation(IEnumerable values, IStringConcatenator stringConcatenator)
        {
            return ConvertToStringRepresentation(values, DefaultCulture, stringConcatenator);
        }

        /// <summary>Converts the given value list to a string.</summary>
        /// <param name="values">Values to convert to string.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <param name="stringConcatenator">The concatenator which is used to build the string.</param>
        /// <returns>String representation of the given value list.</returns>
        public static string ConvertToStringRepresentation(IEnumerable values, CultureInfo culture, IStringConcatenator stringConcatenator)
        {
            string[] array = ConvertToEnumerable<string>(values).UsingCulture(culture).ToArray();
            return stringConcatenator.Concatenate(array);
        }

        /// <summary>
        /// Checks whether the given value represents null.
        /// The DBNull.Value is treated as null.
        /// This comes handy if conversion is applied to values coming from or sending to a database via ADO.Net.
        /// </summary>
        private static bool ValueRepresentsNull(object value)
        {
            if (value is object)
            {
                return value == DBNull.Value;
            }

            return true;
        }

        /// <summary>
        /// Returns the default value of the given type.
        /// ValueTypes always have a parameterless constructor.
        /// The default value of other types is always null.
        /// </summary>
        private static object GetDefaultValueOfType(TypeInfo type)
        {
            if (!type.IsValueType)
            {
                return null;
            }

            return Activator.CreateInstance(type.AsType());
        }

        private static bool IsGenericNullable(TypeInfo type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();
            }

            return false;
        }

        private static Type GetUnderlyingType(Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        private static bool IsWhiteSpace(string value)
        {
            for (int index = 0; index < value.Length; ++index)
            {
                if (!char.IsWhiteSpace(value[index]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Controls an conversion iteration.</summary>
        /// <typeparam name="T">The type to which the elements of the source are converted.</typeparam>
        public class EnumerableConversion<T> : IEnumerable<T>, IEnumerable
        {
            private readonly Type _destinationType = typeof(T);
            private readonly IEnumerable _valuesToConvert;
            private ConversionOptions _conversionOptions = ConversionOptions.EnhancedTypicalValues;
            private CultureInfo _culture;
            private bool _ignoreNullElements;
            private bool _ignoreNonConvertibleElements;

            private CultureInfo Culture => _culture ?? DefaultCulture;

            internal EnumerableConversion(IEnumerable values, Type destinationType)
              : this(values)
            {
                _destinationType = destinationType;
            }

            internal EnumerableConversion(IEnumerable values)
            {
                _valuesToConvert = values;
            }

            /// <summary>Defines the culture used for conversion.</summary>
            /// <param name="culture">The CultureInfo to use as the current culture.</param>
            /// <returns>This instance.</returns>
            public EnumerableConversion<T> UsingCulture(CultureInfo culture)
            {
                _culture = culture;
                return this;
            }

            /// <summary>Defines options used for conversion.</summary>
            /// <param name="options">The options which are used for conversion.</param>
            /// <returns>This instance.</returns>
            public EnumerableConversion<T> UsingConversionOptions(ConversionOptions options)
            {
                _conversionOptions = options;
                return this;
            }

            /// <summary>
            /// Use this option to ignore non convertible values without throwing an exception.
            /// </summary>
            /// <returns>This instance.</returns>
            public EnumerableConversion<T> IgnoringNonConvertibleElements()
            {
                _ignoreNonConvertibleElements = true;
                return this;
            }

            /// <summary>Use this option to ignore null values.</summary>
            /// <returns>This instance.</returns>
            public EnumerableConversion<T> IgnoringNullElements()
            {
                _ignoreNullElements = true;
                return this;
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection of converted values.
            /// A return value indicates whether the operation succeeded.
            /// </summary>
            /// <param name="result">Enumerator that iterates through the collection of converted values if the operation succeeded.</param>
            /// <returns>true on success; otherwise, false.</returns>
            public bool Try(out IEnumerable<T> result)
            {
                return TryConvert(out result);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection of converted values.
            /// </summary>
            /// <returns>Enumerator that iterates through the collection of converted values.</returns>
            public IEnumerator<T> GetEnumerator()
            {
                IEnumerable<T> result;
                Exception exception;
                if (TryConvert(out result, out exception))
                {
                    return result.GetEnumerator();
                }

                throw exception;
            }

            private bool TryConvert(out IEnumerable<T> result)
            {
                Exception exception;
                return TryConvert(out result, out exception);
            }

            private bool TryConvert(out IEnumerable<T> result, out Exception exception)
            {
                List<T> objList = new List<T>();
                foreach (object valueToConvert in GetValuesToConvert())
                {
                    if (valueToConvert is object || !_ignoreNullElements)
                    {
                        object result1;
                        if (!UniversalTypeConverter.TryConvert(valueToConvert, _destinationType, out result1, Culture, _conversionOptions))
                        {
                            if (!_ignoreNonConvertibleElements)
                            {
                                result = null;
                                exception = new InvalidConversionException(valueToConvert, _destinationType);
                                return false;
                            }
                        }
                        else
                        {
                            objList.Add((T)result1);
                        }
                    }
                }
                result = objList;
                exception = null;
                return true;
            }

            /// <summary>Gets a list of the values to convert.</summary>
            /// <returns>List of values to convert.</returns>
            protected virtual IEnumerable GetValuesToConvert()
            {
                return _valuesToConvert;
            }
        }

        /// <summary>Controls an conversion iteration.</summary>
        /// <typeparam name="T">The type to which the elements of the source are converted.</typeparam>
        public class EnumerableStringConversion<T> : EnumerableConversion<T>
        {
            private string[] _nullValues = new string[1]
            {
                ".null."
            };
            private bool _ignoreEmptyElements;
            private bool _trimStart;
            private bool _trimEnd;

            internal EnumerableStringConversion(string valueList, IStringSplitter stringSplitter)
              : base(stringSplitter.Split(valueList))
            {
            }

            internal EnumerableStringConversion(string valueList, Type destinationType, IStringSplitter stringSplitter)
              : base(stringSplitter.Split(valueList), destinationType)
            {
            }

            /// <summary>
            /// Use this option to ignore empty strings after splitting.
            /// </summary>
            /// <returns>This instance.</returns>
            public EnumerableStringConversion<T> IgnoringEmptyElements()
            {
                _ignoreEmptyElements = true;
                return this;
            }

            /// <summary>
            /// Use this option to trim the start of the splitted strings.
            /// </summary>
            /// <returns>This instance.</returns>
            public EnumerableStringConversion<T> TrimmingStartOfElements()
            {
                _trimStart = true;
                return this;
            }

            /// <summary>
            /// Use this option to trim the end of the splitted strings.
            /// </summary>
            /// <returns>This instance.</returns>
            public EnumerableStringConversion<T> TrimmingEndOfElements()
            {
                _trimEnd = true;
                return this;
            }

            /// <summary>
            /// Defines strings which are treated as null after splitting.
            /// </summary>
            /// <param name="nullValues">List of null values.</param>
            /// <returns>This instance.</returns>
            public EnumerableStringConversion<T> WithNullBeing(params string[] nullValues)
            {
                _nullValues = nullValues;
                return this;
            }

            /// <summary>Gets a list of the values to convert.</summary>
            /// <returns>List of values to convert.</returns>
            protected override IEnumerable GetValuesToConvert()
            {
                List<string> stringList = new List<string>();
                foreach (string str in base.GetValuesToConvert())
                {
                    string convert = PreProcessValueToConvert(str);
                    if (!ValueShouldBeIgnored(convert))
                    {
                        stringList.Add(convert);
                    }
                }
                return stringList;
            }

            private string PreProcessValueToConvert(string value)
            {
                string str = value;
                if (_trimStart)
                {
                    str = str.TrimStart(Array.Empty<char>());
                }

                if (_trimEnd)
                {
                    str = str.TrimEnd(Array.Empty<char>());
                }

                return ValueOrNull(str);
            }

            private bool ValueShouldBeIgnored(string valueToConvert)
            {
                return valueToConvert == string.Empty && _ignoreEmptyElements;
            }

            private string ValueOrNull(string value)
            {
                if (_nullValues is null)
                {
                    return value;
                }

                string str = value;
                if (((IEnumerable<string>)_nullValues).Contains(value))
                {
                    str = null;
                }

                return str;
            }
        }
    }
}