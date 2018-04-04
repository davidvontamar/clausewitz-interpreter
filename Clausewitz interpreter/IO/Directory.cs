using System.Collections.Generic;
namespace Clausewitz.IO
{
	/// <summary>
	///     Corresponds to a file directory. (Renamed from 'Directory' to 'Folder' due to name conflicts with
	///     'System.IO.Directory', Then renamed again back to 'Directory' after using an alias names for the .NET static
	///     classes.
	/// </summary>
	public class Directory : IExplorable
	{
		/// <summary>Primary constructor</summary>
		/// <param name="parent">Parent directory.</param>
		/// <param name="name">Directory name.</param>
		internal Directory(Directory parent, string name)
		{
			Name = name;
			Parent = parent;
		}

		/// <inheritdoc />
		public string Address
		{
			get
			{
				return this.GetAddress();
			}
		}

		/// <summary>Directory name.</summary>
		public string Name
		{
			get;
			set;
		}

		/// <inheritdoc />
		public Directory Parent
		{
			get;
		}

		/// <summary>Creates a new directory within this directory.</summary>
		/// <param name="name"></param>
		public void NewDirectory(string name)
		{
			Directories.Add(new Directory(this, name));
		}

		/// <summary>Creates a new file within this directory.</summary>
		/// <param name="name">File name without extension</param>
		/// <param name="extension">File extension.</param>
		public void NewFile(string name, File.Extensions extension)
		{
			Files.Add(new File(this, name, extension));
		}

		/// <summary>Sub-directories</summary>
		public readonly List<Directory> Directories = new List<Directory>();

		/// <summary>Files.</summary>
		public readonly List<File> Files = new List<File>();
	}
}
