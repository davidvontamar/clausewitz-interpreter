namespace Clausewitz.Constructs
{
	/// <summary>A single token, sometimes a number, a string or just a symbol.</summary>
	public class Token : Construct
	{
		#region Constructors
		/// <summary>
		/// Primary constructor.
		/// </summary>
		/// <param name="scope">Parent scope.</param>
		/// <param name="value">The token itself.</param>
		internal Token(Scope scope, string value) : base(Types.Token, scope)
		{
			Value = value;
		}
		#endregion

		#region Fields
		/// <summary>The actual symbol/value of the token.</summary>
		public string Value;
		#endregion
	}
}
