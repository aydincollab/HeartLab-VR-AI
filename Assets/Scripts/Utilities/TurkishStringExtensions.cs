using System;
using System.Globalization;

namespace HeartLabVR.Utilities
{
    /// <summary>
    /// Extension methods for Turkish language-aware string operations
    /// Provides proper Turkish culture support for string transformations
    /// </summary>
    public static class TurkishStringExtensions
    {
        private static readonly CultureInfo TurkishCulture = new CultureInfo("tr-TR");

        /// <summary>
        /// Converts string to lowercase using Turkish culture rules
        /// Handles Turkish-specific characters like İ/i and I/ı correctly
        /// </summary>
        /// <param name="text">String to convert</param>
        /// <returns>Lowercase string using Turkish culture</returns>
        public static string ToLowerTurkish(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text.ToLower(TurkishCulture);
        }

        /// <summary>
        /// Converts string to uppercase using Turkish culture rules
        /// Handles Turkish-specific characters like İ/i and I/ı correctly
        /// </summary>
        /// <param name="text">String to convert</param>
        /// <returns>Uppercase string using Turkish culture</returns>
        public static string ToUpperTurkish(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text.ToUpper(TurkishCulture);
        }

        /// <summary>
        /// Checks if a string contains Turkish-specific characters
        /// </summary>
        /// <param name="text">String to check</param>
        /// <returns>True if contains Turkish characters</returns>
        public static bool ContainsTurkish(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Check for Turkish-specific characters
            foreach (char c in text)
            {
                if (c == 'ç' || c == 'Ç' || c == 'ğ' || c == 'Ğ' || 
                    c == 'ı' || c == 'I' || c == 'İ' || c == 'i' ||
                    c == 'ö' || c == 'Ö' || c == 'ş' || c == 'Ş' ||
                    c == 'ü' || c == 'Ü')
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Performs culture-aware string comparison using Turkish culture
        /// </summary>
        /// <param name="text">First string</param>
        /// <param name="other">Second string</param>
        /// <param name="ignoreCase">Whether to ignore case</param>
        /// <returns>True if strings are equal using Turkish culture rules</returns>
        public static bool EqualsTurkish(this string text, string other, bool ignoreCase = true)
        {
            if (text == null && other == null)
                return true;

            if (text == null || other == null)
                return false;

            StringComparison comparison = ignoreCase
                ? StringComparison.CurrentCultureIgnoreCase
                : StringComparison.CurrentCulture;

            return string.Compare(text, other, TurkishCulture, 
                ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) == 0;
        }

        /// <summary>
        /// Performs culture-aware string contains check using Turkish culture
        /// </summary>
        /// <param name="text">Source string</param>
        /// <param name="value">Value to search for</param>
        /// <param name="ignoreCase">Whether to ignore case</param>
        /// <returns>True if source contains value using Turkish culture rules</returns>
        public static bool ContainsTurkish(this string text, string value, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
                return false;

            if (ignoreCase)
            {
                return text.ToLowerTurkish().Contains(value.ToLowerTurkish());
            }

            return text.Contains(value);
        }
    }
}