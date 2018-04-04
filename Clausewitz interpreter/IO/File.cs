using System.IO;
using Clausewitz.Constructs;
namespace Clausewitz.IO
{
	/// <summary>An extended type of scope, which enforces file name and parent directory.</summary>
	public class File : Scope, IExplorable
	{
		/// <summary>Primary constructor.</summary>
		/// <param name="parent">Parent directory.</param>
		/// <param name="name">File name without extension.</param>
		/// <param name="extension">File extension.</param>
		internal File(Directory parent, string name, Extensions extension) : base(name)
		{
			Name = name;
			Parent = parent;
			Extension = extension;
		}

		/// <inheritdoc />
		public string Address
		{
			get
			{
				return this.GetAddress();
			}
		}

		/// <inheritdoc />
		public new Directory Parent
		{
			get;
		}

		/// <summary>
		/// File extension.
		/// </summary>
		public Extensions Extension
		{
			get;
		}

		/// <summary>Relevant file extensions.</summary>
		public enum Extensions
		{
			Txt,
			Csv
		}
	}
}
