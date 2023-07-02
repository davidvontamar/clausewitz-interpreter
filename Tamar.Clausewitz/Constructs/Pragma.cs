using System.Collections.Generic;

namespace Tamar.Clausewitz.Constructs;

/// <summary>
///     Every Clausewitz construct may have pragmas within the associated comments.
///     Each pragma includes a set of
///     keywords.
/// </summary>
public struct Pragma
{
	/// <summary>User-friendly constructor.</summary>
	/// <param name="keywords">Keywords.</param>
	public Pragma(params string[] keywords)
	{
		Keywords = new HashSet<string>(keywords);
	}

	/// <summary>Primary constructor.</summary>
	/// <param name="keywords">Keywords.</param>
	public Pragma(HashSet<string> keywords)
	{
		Keywords = keywords;
	}

	/// <summary>Checks if a pragma has all of the specified keywords.</summary>
	/// <param name="keywords">Keywords.</param>
	/// <returns>Boolean.</returns>
	public bool Contains(IEnumerable<string> keywords)
	{
		return Keywords.IsSupersetOf(keywords.FormatKeywords());
	}

	/// <summary>Checks if a pragma has all of the specified keywords.</summary>
	/// <param name="keywords">Keywords.</param>
	/// <returns>Boolean.</returns>
	public bool Contains(params string[] keywords)
	{
		return Keywords.IsSupersetOf(keywords.FormatKeywords());
	}

	/// <summary>
	///     Keywords are separated by spaces within each pragma, and their order does not
	///     matter.
	/// </summary>
	public readonly HashSet<string> Keywords;
}