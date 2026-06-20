using System.Globalization;
using System.Text;

namespace SearchEngine.Core.Analysis
{
    public static class Normalizer
    {
        /// <summary>
        /// Normalizes a token by trimming surrounding punctuation, converting to
        /// lower-case and removing diacritics.
        /// </summary>
        public static string Normalize(ReadOnlySpan<char> token)
        {
            if (token.IsEmpty)
                return string.Empty;

            int start = 0;
            int end = token.Length - 1;

            // Trimming punctuation at the start and end
            while (start <= end && IsPunctuation(token[start]))
                start++;
            while (end >= start && IsPunctuation(token[end]))
                end--;

            if (start > end)
                return string.Empty;

            var sb = new StringBuilder(end - start + 1);
            for (int i = start; i <= end; i++)
            {
                char c = char.ToLowerInvariant(token[i]);

                c = RemoveDiacritics(c);

                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
            }

            if (sb.Length == 0)
                return string.Empty;

            return sb.ToString();
        }

        private static bool IsPunctuation(char c) =>
            c == '.'
            || c == ','
            || c == ':'
            || c == ';'
            || c == '!'
            || c == '?'
            || c == '('
            || c == ')'
            || c == '['
            || c == ']'
            || c == '{'
            || c == '}'
            || c == '"'
            || c == '\''
            || c == '�'
            || c == '-'
            || c == '_';

        private static char RemoveDiacritics(char c)
        {
            var normalizedString = c.ToString().Normalize(NormalizationForm.FormD);

            foreach (var ch in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                {
                    return ch;
                }
            }
            return c;
        }
    }
}
