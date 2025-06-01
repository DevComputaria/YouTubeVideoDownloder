using System.Text;
using System.Text.RegularExpressions;

namespace YouTubeVideoDownload.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="string"/> operations.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Generates a URL-friendly slug from a given string.
        /// The process involves removing accents, converting to lowercase,
        /// removing invalid characters, collapsing and trimming whitespace,
        /// truncating to a maximum length (45 characters), and replacing spaces with hyphens.
        /// </summary>
        /// <param name="phrase">The string to convert into a slug.</param>
        /// <returns>A URL-friendly slug. Returns an empty string if the input phrase is null or empty.</returns>
        public static string GenerateSlug(this string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
                return string.Empty;

            string str = phrase.RemoveAccent().ToLower();
            // Replace invalid characters (anything not a letter, number, space, or hyphen) with empty string
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // Convert multiple spaces into a single space
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // Truncate to 45 characters if longer
            str = str.Length <= 45 ? str : str.Substring(0, 45).TrimEnd(); // TrimEnd to avoid trailing space before hyphen
            // Replace spaces with hyphens
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }

        /// <summary>
        /// Removes diacritics (accents) from a string by converting it to its closest ASCII representation.
        /// Note: This method uses Cyrillic to ASCII conversion, which might not be universally applicable
        /// for all accented characters but covers a common range.
        /// </summary>
        /// <param name="txt">The string from which to remove accents.</param>
        /// <returns>The string with accents removed. Returns an empty string if the input is null or empty.</returns>
        public static string RemoveAccent(this string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return string.Empty;

            // The "Cyrillic" encoding page for ASCII conversion is a common trick but might not be perfect for all languages.
            // For more robust Unicode normalization, consider: txt.Normalize(NormalizationForm.FormD) and then removing non-spacing marks.
            byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}