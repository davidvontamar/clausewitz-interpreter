namespace Clausewitz.Constructs
{
	/// <summary>
	///     Any statement which includes the assignment operator '=' in Clausewitz. Including most commands, conditions
	///     and triggers which come in a single line.
	/// </summary>
	public class Binding : Construct
	{
		#region Constructors
		/// <summary>Primary constructor.</summary>
		/// <param name="scope">Parent scope.</param>
		/// <param name="name">Left side.</param>
		/// <param name="value">Right side.</param>
		internal Binding(Scope scope, string name, string value) : base(Types.Binding, scope)
		{
			Name = name;
			Value = value;
		}
		#endregion

		#region Fields
		/// <summary>Left side.</summary>
		public string Name;

		/// <summary>Right side.</summary>
		public string Value;
		#endregion
	}
}
