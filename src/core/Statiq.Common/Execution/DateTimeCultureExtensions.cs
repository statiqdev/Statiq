using System;
using System.Globalization;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions for working with input and output date cultures.
    /// </summary>
    public static class DateTimeCultureExtensions
    {
        /// <summary>
        /// Attempts to parse an input date using the input date culture setting.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="s">The string to parse.</param>
        /// <param name="result">The resulting <see cref="DateTime"/> instance.</param>
        /// <returns><c>true</c> if the input string could be parsed, <c>false</c> otherwise.</returns>
        public static bool TryParseInputDateTime(this IExecutionContext context, string s, out DateTime result) =>
            DateTime.TryParse(s, context.GetDateTimeInputCulture(), DateTimeStyles.None, out result);

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> for the date input culture.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The date input culture.</returns>
        public static CultureInfo GetDateTimeInputCulture(this IExecutionContext context)
        {
            if (context?.Settings.ContainsKey(Keys.DateTimeInputCulture) != true)
            {
                return CultureInfo.CurrentCulture;
            }
            object value = context.Settings.Get(Keys.DateTimeInputCulture);
            return value as CultureInfo ?? CultureInfo.GetCultureInfo(value.ToString());
        }

        /// <summary>
        /// Gets a short date display string using the date display culture setting.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to generate a string for.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="targetCulture">The culture that should be used if the date display setting isn't provided. If the
        /// current culture is of the same family, then it will be used. If not, the specified target culture will be used.</param>
        /// <returns>A short date display string.</returns>
        public static string ToShortDateString(this in DateTime dateTime, IExecutionContext context, string targetCulture = "en-GB")
        {
            CultureInfo culture = context.GetDateTimeDisplayCulture(targetCulture);
            return dateTime.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
        }

        /// <summary>
        /// Gets a long date display string using the date display culture setting.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to generate a string for.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="targetCulture">The culture that should be used if the date display setting isn't provided. If the
        /// current culture is of the same family, then it will be used. If not, the specified target culture will be used.</param>
        /// <returns>A long date display string.</returns>
        public static string ToLongDateString(this in DateTime dateTime, IExecutionContext context, string targetCulture = "en-GB")
        {
            CultureInfo culture = context.GetDateTimeDisplayCulture(targetCulture);
            return dateTime.ToString(culture.DateTimeFormat.LongDatePattern, culture);
        }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> for the date display culture.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="targetCulture">The culture that should be used if the date display setting isn't provided. If the
        /// current culture is of the same family, then it will be used. If not, the specified target culture will be used.</param>
        /// <returns>The date display culture.</returns>
        public static CultureInfo GetDateTimeDisplayCulture(this IExecutionContext context, string targetCulture = "en-GB")
        {
            // Get the culture info
            CultureInfo cultureInfo = null;
            if (context.Settings.ContainsKey(Keys.DateTimeDisplayCulture))
            {
                string cultureName = context.Settings.GetString(Keys.DateTimeDisplayCulture);
                if (!string.IsNullOrWhiteSpace(cultureName))
                {
                    cultureInfo = CultureInfo.GetCultureInfo(cultureName);
                }
            }
            if (cultureInfo is null)
            {
                CultureInfo target = CultureInfo.GetCultureInfo(targetCulture);
                cultureInfo = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals(target.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase)
                    ? CultureInfo.CurrentCulture : target;
            }

            return cultureInfo;
        }
    }
}