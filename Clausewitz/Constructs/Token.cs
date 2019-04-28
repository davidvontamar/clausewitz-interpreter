namespace Tamar.Clausewitz.Constructs
{
	/// <summary>A single token, sometimes a number, a string or just a symbol.</summary>
	public class Token: Construct
	{
		/// <summary>Primary constructor.</summary>
		/// <param name="parent">Parent scope.</param>
		/// <param name="value">The token itself.</param>
		internal Token(Scope parent, string value): base(parent)
		{
			Value = value;
		}

		/// <summary>The actual symbol/value of the token.</summary>
		public string Value;
	}
}