using System;

namespace CommonGameFramework.Extensions
{
    /// <summary>string 扩展。</summary>
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        /// <summary>超过 maxLength 时截断并追加 suffix（默认 "..."）。</summary>
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value) || maxLength < 0 || value.Length <= maxLength)
                return value;

            if (string.IsNullOrEmpty(suffix) || suffix.Length >= maxLength)
                return value.Substring(0, maxLength);

            return value.Substring(0, maxLength - suffix.Length) + suffix;
        }
    }
}
