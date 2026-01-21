using System.Text;

namespace Lithium.Server.Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Checks if the string contains only digits (0-9).
    /// </summary>
    public static bool IsDigits(this string str)
    {
        if (string.IsNullOrEmpty(str)) return false;
        foreach (char c in str)
        {
            if (!char.IsDigit(c)) return false;
        }
        return true;
    }
    
    /// <summary>
    /// Checks if the string contains only alphanumeric characters and hyphens.
    /// </summary>
    public static bool IsAlphaNumericHyphen(this string str)
    {
        if (string.IsNullOrEmpty(str)) return false;
        foreach (char c in str)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '-') return false;
        }
        return true;
    }

    /// <summary>
    /// Capitalizes the first letter of each word separated by the delimiter.
    /// </summary>
    public static string Capitalize(this string str, char delimiter = ' ')
    {
        if (string.IsNullOrEmpty(str)) return str;
        
        // Use TextInfo for TitleCase, but it might behave slightly differently than the manual impl regarding arbitrary delimiters.
        // The Java impl manually capitalizes the char immediately following a delimiter.
        // Let's implement manually to match behavior.
        
        var sb = new StringBuilder(str.Length);
        bool capitalizeNext = true;
        
        foreach (char c in str)
        {
            if (capitalizeNext)
            {
                sb.Append(char.ToUpper(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(c);
            }
            
            if (c == delimiter)
            {
                capitalizeNext = true;
            }
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Humanizes a TimeSpan into a string like "2d 5h 30m".
    /// </summary>
    public static string Humanize(this TimeSpan duration, bool includeSeconds = false)
    {
        var sb = new StringBuilder();
        
        if (duration.Days > 0) sb.Append($"{duration.Days}d ");
        if (duration.Hours > 0 || duration.Days > 0) sb.Append($"{duration.Hours}h ");
        sb.Append($"{duration.Minutes}m");
        
        if (includeSeconds)
        {
            sb.Append($" {duration.Seconds}s");
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Computes the Levenshtein distance between two strings.
    /// </summary>
    public static int LevenshteinDistance(this string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
        if (string.IsNullOrEmpty(t)) return s.Length;

        int n = s.Length;
        int m = t.Length;
        var d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }
    
    /// <summary>
    /// Computes a fuzzy match score between a term and a query. Higher is better.
    /// Adapted from Hytale's StringCompareUtil.
    /// </summary>
    public static int FuzzyDistance(this string term, string query)
    {
        if (term == null || query == null) throw new ArgumentNullException();
        
        // Using invariant culture for consistency, though Java used Locale.ENGLISH (which is essentially Invariant mostly).
        var termLower = term.ToLowerInvariant();
        var queryLower = query.ToLowerInvariant();
        
        int score = 0;
        int termIndex = 0;
        int previousMatchingCharacterIndex = int.MinValue;

        for (int queryIndex = 0; queryIndex < queryLower.Length; queryIndex++)
        {
            char queryChar = queryLower[queryIndex];
            bool termCharacterMatchFound = false;

            for (; termIndex < termLower.Length && !termCharacterMatchFound; termIndex++)
            {
                char termChar = termLower[termIndex];
                if (queryChar == termChar)
                {
                    score++;
                    
                    // Reward consecutive matches
                    if (previousMatchingCharacterIndex + 1 == termIndex)
                    {
                        score += 2;
                    }

                    previousMatchingCharacterIndex = termIndex;
                    termCharacterMatchFound = true;
                }
            }
        }

        return score;
    }
}
