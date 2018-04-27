using System;
using System.IO;
using System.Linq;
namespace Clausewitz.IO
{
	/// <summary>
	///     Extension class for various interfaces. These extensions are used only within this library to implement a
	///     similar pattern to Multiple-Inheritances in C#. Implement corresponding properties within the derived classes of
	///     these interfaces that call these extensions.
	/// </summary>
	internal static class Extensions
	{
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
					return Environment.CurrentDirectory + '\\' + address;
				if (currentExplorable.GetType() == typeof(Directory))
					address = currentExplorable.Name + '\\' + address;
				else
					address = currentExplorable.Name;
				currentExplorable = currentExplorable.Parent;
			}
		}

		/// <summary>Checks if the given address is a fully qualified path or a relative path.</summary>
		/// <param name="address">Address.</param>
		/// <returns>Boolean.</returns>
		internal static bool IsFullAddress(this string address)
		{
			if (string.IsNullOrWhiteSpace(address))
				return false;
			if (address.IndexOfAny(Path.GetInvalidPathChars().ToArray()) != -1)
				return false;
			if (!Path.IsPathRooted(address))
				return false;
			return Path.GetPathRoot(address) != Path.DirectorySeparatorChar.ToString();
		}
	}
}
