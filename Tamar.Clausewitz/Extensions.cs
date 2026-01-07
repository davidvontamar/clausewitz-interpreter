using System.Collections.Generic;
using Tamar.Clausewitz.Constructs;

namespace Tamar.Clausewitz;

/// <summary>
/// Extension class for all language constructs.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Returns true if a collection of pragmas has the requested pragma with the specified options.
    /// </summary>
    public static bool ContainsOptions(this IEnumerable<Pragma> pragmas, params string[] options)
    {
        foreach (var pragma in pragmas)
            if (pragma.ContainsOptions(options))
                return true;
        return false;
    }

    /// <summary>
    /// Formats a pragma option to lower case and trims it.
    /// </summary>
    internal static string FormatOption(this string option)
    {
        return option.Trim().ToLower();
    }

    /// <summary>
    /// Formats all options with lazy evaluation.
    /// </summary>
    internal static IEnumerable<string> FormatKeywords(this IEnumerable<string> keywords)
    {
        foreach (var keyword in keywords)
            yield return keyword.FormatOption();
    }
}