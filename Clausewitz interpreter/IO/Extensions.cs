using System;
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
				currentExplorable = explorable.Parent;
			}
		}
	}
}
