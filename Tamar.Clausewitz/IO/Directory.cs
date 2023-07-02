using System.Collections.Generic;

namespace Tamar.Clausewitz.IO;

/// <summary>
///     Corresponds to a file directory. (Renamed from 'Directory' to 'Folder' due to
///     name conflicts with
///     'System.IO.Directory', Then renamed again back to 'Directory' after using an
///     alias names for the .NET static
///     classes.
/// </summary>
public class Directory : IExplorable
{
	/// <summary>Sub-directories</summary>
	public readonly List<Directory> Directories = new();

	/// <summary>Files.</summary>
	public readonly List<FileScope> Files = new();

	/// <summary>Primary constructor</summary>
	/// <param name="parent">Parent directory.</param>
	/// <param name="name">Directory name.</param>
	internal Directory(Directory parent, string name)
	{
		Name = name;
		Parent = parent;
	}

	/// <summary>
	///     Returns all directories and then all files within this directory in a single
	///     list.
	/// </summary>
	public IEnumerable<IExplorable> Explorables
	{
		get
		{
			var explorables = new List<IExplorable>();
			explorables.AddRange(Directories);
			explorables.AddRange(Files);
			return explorables;
		}
	}

	/// <summary>
	///     Returns true if this directory has no parent. (Typically "C:\")
	/// </summary>
	public bool IsRoot => Parent == null;

	/// <inheritdoc />
	public string Address => this.GetAddress();

	/// <summary>Directory name.</summary>
	public string Name { get; set; }

	/// <inheritdoc />
	public Directory Parent { get; internal set; }

	/// <summary>
	///     Creates a new directory within this directory. (Automatically assigns the
	///     parent)
	/// </summary>
	/// <param name="name">Directory name.</param>
	/// <returns>New directory.</returns>
	public Directory NewDirectory(string name)
	{
		var directory = new Directory(this, name);
		Directories.Add(directory);
		return directory;
	}

	/// <summary>
	///     Creates a new file within this directory. (Automatically assigns the
	///     parent)
	/// </summary>
	/// <param name="name">File name with extension</param>
	/// <returns>New file.</returns>
	public FileScope NewFile(string name)
	{
		var file = new FileScope(this, name);
		Files.Add(file);
		return file;
	}
}