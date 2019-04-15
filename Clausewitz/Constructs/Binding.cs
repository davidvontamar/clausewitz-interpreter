namespace Clausewitz.Constructs
{
	/// <summary>
	/// Any statement which includes the assignment operator '=' in Clausewitz.
	/// Including most commands, conditions
	/// and triggers which come in a single line.
	/// </summary>
	public class Binding : Construct
	{
		/// <summary>Primary constructor.</summary>
		/// <param name="parent">Parent scope.</param>
		/// <param name="name">Left side.</param>
		/// <param name="value">Right side.</param>
		internal Binding(Scope parent, string name, string value) : base(parent)
		{
			Name = name;
			Value = value;
		}

		/// <summary>Left side.</summary>
		public string Name;

		/// <summary>Right side.</summary>
		public string Value;
	}
}