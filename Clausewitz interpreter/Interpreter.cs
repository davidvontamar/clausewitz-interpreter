using System;
using System.IO;
using Clausewitz.Constructs;
using Clausewitz.IO;
using Directory = Clausewitz.IO.Directory;
using File = Clausewitz.IO.File;
namespace Clausewitz
{
	/// <summary>The Clausewitz interpreter.</summary>
	public static class Interpreter
	{
		/// <summary>Reads & interprets all files or data found in the given address.</summary>
		/// <param name="address">Relative or fully qualified path.</param>
		/// <param name="parent">Parent directory.</param>
		public static IExplorable Read(string address, Directory parent = null)
		{
			// This checks whether the address is local or full:
			if (!address.IsFullAddress())
				address = Environment.CurrentDirectory + address;

			// Might be a file or a directory.
			IExplorable explorable = null;

			// Check whether a file or a directory and act accordingly:
			var extension = Path.GetExtension(address).ToLower();
			if (extension != string.Empty)
			{
				// Files:
				if (extension == "txt")
					explorable = new File(parent, Path.GetFileNameWithoutExtension(address), File.Extensions.Txt);
				else if (extension == "csv")
					explorable = new File(parent, Path.GetFileNameWithoutExtension(address), File.Extensions.Csv);
			}
			else
			{
				// Directories:
				if (!System.IO.Directory.Exists(address))
					System.IO.Directory.CreateDirectory(address);
				Log.Send(Log.Message.Types.Info, "Created directory.", address);

				// Explore directory:
				foreach (var entry in System.IO.Directory.GetFileSystemEntries(address))
					Read(entry);
				explorable = new Directory(parent, Path.GetDirectoryName(address));
			}
			return explorable;
		}

		internal static void Interpret(Scope scope)
		{ }
	}
}
