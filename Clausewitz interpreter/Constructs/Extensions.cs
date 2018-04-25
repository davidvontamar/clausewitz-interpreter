using System.Collections.Generic;
namespace Clausewitz.Constructs
{
	/// <summary>
	/// Extension class for all language constructs.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Checks if a collection of pragmas has the requested pragma with the specified keywords.
		/// </summary>
		/// <param name="pragmas">Extended.</param>
		/// <param name="keywords">Keywords (all keywords).</param>
		/// <returns>Returns true if a pragma with the said keywords was found.</returns>
		public static bool Contains(this IEnumerable<Pragma> pragmas, params string[] keywords)
		{
			foreach (var pragma in pragmas)
				if (pragma.Contains(keywords))
					return true;
			return false;
		}

		/// <summary>
		/// Formats a keyword to lower case and trims it.
		/// </summary>
		/// <param name="keyword">Extended.</param>
		/// <returns>Formatted</returns>
		internal static string FormatKeyword(this string keyword)
		{
			return keyword.Trim().ToLower();
		}

		/// <summary>
		/// Formats all keywords with lazy evaluation.
		/// </summary>
		/// <param name="keywords">Extended.</param>
		/// <returns>Formatted.</returns>
		internal static IEnumerable<string> FormatKeywords(this IEnumerable<string> keywords)
		{
			foreach (var keyword in keywords)
				yield return keyword.FormatKeyword();
		}
	}
}
