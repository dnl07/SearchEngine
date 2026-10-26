using System.Globalization;
using System.Text;

namespace SearchEngine.Core.Analysis
{
    public static class Normalizer
    {
        /// <summary>
        /// Lower-cases the word, removes diacritics, replaces ß with ss and splits it at every
        /// character that is not a letter or digit.
        /// </summary>
        public static List<string> SplitParts(string word)
        {
            var parts = new List<string>();
            var current = new StringBuilder(word.Length);

            foreach (char raw in word)
            {
                char c = char.ToLowerInvariant(raw);

                if (c == 'ß')
                {
                    current.Append("ss");
                    continue;
                }

                c = RemoveDiacritics(c);

                if (char.IsLetterOrDigit(c))
                {
                    current.Append(c);
                }
                else if (current.Length > 0)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
            }

            if (current.Length > 0)
            {
                parts.Add(current.ToString());
            }

            return parts;
        }

        /// <summary>
        /// Returns the word normalized like <see cref="SplitParts"/>, without the separating
        /// characters.
        /// </summary>
        public static string Normalize(string word) => string.Concat(SplitParts(word));

        /// <summary>
        /// Returns the base letter of a character with diacritics, or the character itself.
        /// </summary>
        private static char RemoveDiacritics(char c)
        {
            if (c < 128)
                return c;

            foreach (char decomposed in c.ToString().Normalize(NormalizationForm.FormD))
            {
                if (
                    CharUnicodeInfo.GetUnicodeCategory(decomposed) != UnicodeCategory.NonSpacingMark
                )
                {
                    return decomposed;
                }
            }

            return c;
        }
    }
}
