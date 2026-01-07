using System.Collections.Generic;

namespace Tamar.Clausewitz.Constructs;

/// <summary>
/// Every Clausewitz construct may have pragmas within the associated comments.
/// Each pragma includes a set of options.
/// </summary>
public struct Pragma
{
    /// <summary>User-friendly constructor.</summary>
    public Pragma(params string[] options)
    {
        Options = new HashSet<string>(options);
    }

    /// <summary>Primary constructor.</summary>
    public Pragma(HashSet<string> options)
    {
        Options = options;
    }

    /// <summary>
    /// Options are separated by spaces within each pragma, and their order does not matter.
    /// </summary>
    public readonly HashSet<string> Options;

    /// <summary>Returns true if a pragma has all of the specified options.</summary>
    public bool ContainsOptions(IEnumerable<string> options)
    {
        return Options.IsSupersetOf(options.FormatKeywords());
    }

    /// <summary>Returns true if a pragma has all of the specified options.</summary>
    public bool ContainsOptions(params string[] options)
    {
        return Options.IsSupersetOf(options.FormatKeywords());
    }
}