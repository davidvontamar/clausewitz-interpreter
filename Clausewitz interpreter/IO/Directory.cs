using System.Collections.Generic;
namespace Clausewitz.IO
{
	/// <summary>Corresponds to a file directory.</summary>
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

		/// <summary>Sub-directories</summary>
		public List<Directory> Directories;

		/// <summary>Files.</summary>
		public List<File> Files;
	}
}
