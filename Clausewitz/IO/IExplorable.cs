namespace Clausewitz.IO
{
	/// <summary>Scopes which can be explored through a file manager.</summary>
	public interface IExplorable
	{
		/// <summary>Full address.</summary>
		string Address { get; }

		/// <summary>Name.</summary>
		string Name { get; }

		/// <summary>Parent directory.</summary>
		Directory Parent { get; }
	}
}