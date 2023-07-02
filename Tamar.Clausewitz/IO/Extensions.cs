using System;
using System.IO;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace Tamar.Clausewitz.IO;

/// <summary>
///     Extension class for various interfaces. These extensions are used only within
///     this library to implement a
///     similar pattern to Multiple-Inheritances in C#. Implement corresponding
///     properties within the derived classes of
///     these interfaces that call these extensions.
/// </summary>
internal static class Extensions
{
	/// <summary>
	///     Retrieves the parent directory connected to all grandparents of parents without
	///     any sibling entries.
	/// </summary>
	/// <param name="address">Extended.</param>
	/// <returns>Closest parent.</returns>
	public static Directory DefineParents(this string address)
	{
		if (!Path.IsPathFullyQualified(address))
			address = Path.Combine(Environment.CurrentDirectory, address);

		var root = new Directory(null, Path.GetPathRoot(address));
		var parent = root;
		var remaining = address.Remove(0, root.Address.Length);
		while (remaining.Contains(Path.DirectorySeparatorChar))
		{
			var name = remaining.Substring(0, remaining.IndexOf(Path.DirectorySeparatorChar));
			remaining = remaining.Remove(0, name.Length + 1);
			parent = parent.NewDirectory(name);
		}

		return parent;
	}

	/// <summary>
	///     Checks if the the given directory is included somewhere within the extended
	///     directory.
	/// </summary>
	/// <param name="candidate">Extended.</param>
	/// <param name="parent">Suspected parent.</param>
	/// <returns>True if included.</returns>
	public static bool IsSubDirectoryOf(this string candidate, string parent)
	{
		var isChild = false;
		var candidateInfo = new DirectoryInfo(candidate);
		var parentInfo = new DirectoryInfo(parent);

		while (candidateInfo.Parent != null)
			if (candidateInfo.Parent.FullName == parentInfo.FullName)
			{
				isChild = true;
				break;
			}
			else
			{
				candidateInfo = candidateInfo.Parent;
			}

		return isChild;
	}

	/// <summary>Retrieves the full address.</summary>
	/// <param name="explorable">Extended.</param>
	/// <returns>Full address.</returns>
	internal static string GetAddress(this IExplorable explorable)
	{
		var address = string.Empty;
		var currentExplorable = explorable;
		while (true)
		{
			if (currentExplorable == null)
				return address;
			if (currentExplorable.GetType() == typeof(Directory))
				address = Path.Combine(currentExplorable.Name, address);
			else
				address = currentExplorable.Name;
			currentExplorable = currentExplorable.Parent;
		}
	}

	/// <summary>
	///     Ensures the address is fully qualified using the correct directory and drive
	///     separators for each platform.
	/// </summary>
	/// <param name="address">Relative or fully qualified path.</param>
	internal static string ToFullyQualifiedAddress(this string address)
	{
		// Replace slashes with platform specific separators.
		if (Environment.OSVersion.Platform == PlatformID.Unix)
			address = address.Replace('\\', Path.DirectorySeparatorChar);
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			address = address.Replace('/', Path.DirectorySeparatorChar);
		}
		else
		{
			address = address.Replace('/', Path.DirectorySeparatorChar);
			address = address.Replace('\\', Path.DirectorySeparatorChar);
		}

		// Replace pairs of separators with single ones:
		var separatorPair = string.Empty + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar;
		while (address.Contains(separatorPair))
			address = address.Replace(separatorPair,
				string.Empty + Path.DirectorySeparatorChar);

		// This checks whether the address is local or full:
		if (!Path.IsPathFullyQualified(address))
			address = Environment.CurrentDirectory + Path.DirectorySeparatorChar + address;
		return address;
	}
}