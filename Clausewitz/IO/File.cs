using Tamar.Clausewitz.Constructs;

namespace Tamar.Clausewitz.IO
{
	/// <summary>
	/// An extended type of scope, which enforces file name and parent
	/// directory.
	/// </summary>
	public class File: Scope, IExplorable
	{
		/// <summary>Primary constructor.</summary>
		/// <param name="parent">Parent directory.</param>
		/// <param name="name">File name with extension.</param>
		internal File(Directory parent, string name): base(name)
		{
			Name = name;
			Parent = parent;
		}

		/// <inheritdoc />
		public string Address
		{
			get { return this.GetAddress(); }
		}

		/// <inheritdoc />
		public new Directory Parent { get; internal set; }

		/// <summary>
		/// Retrieves all text within this file.
		/// </summary>
		/// <returns>File contents.</returns>
		public string ReadText()
		{
			return System.IO.File.ReadAllText(Address);
		}

		/// <summary>
		/// Writes the given text into this file.
		/// </summary>
		/// <returns></returns>
		internal void WriteText(string data)
		{
			System.IO.File.WriteAllText(Address, data);
		}
	}
}