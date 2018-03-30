using System.Collections.Generic;
namespace Clausewitz.Constructs
{
	/// <summary>Scopes are files, direcotries, and clauses.</summary>
	public class Scope : Construct
	{
		/// <summary>Special scope constructor for files (which has no parent scope, but parent directory).</summary>
		/// <param name="name">Scope name.</param>
		protected Scope(string name) : this(null, name)
		{
			// implemented at primary constructor.
		}

		/// <summary>Primary constructor.</summary>
		/// <param name="parent">Parent scope.</param>
		/// <param name="name">Optional name.</param>
		internal Scope(Scope parent, string name = null) : base(Types.Scope, parent)
		{
			Items = new List<Construct>();
			if (name != null)
				Name = name;
		}

		/// <summary>Optional scope name (not all scopes have names in Clausewitz)</summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>Creates a new binding within this scope.</summary>
		/// <param name="name">Left side.</param>
		/// <param name="value">Right side.</param>
		public void NewBinding(string name, string value)
		{
			Items.Add(new Binding(this, name, value));
		}

		/// <summary>Creates a new scope within this scope.</summary>
		/// <param name="name">Optional name.</param>
		public void NewScope(string name = null)
		{
			Items.Add(new Scope(this, name));
		}

		/// <summary>Creates a new token within this scope.</summary>
		/// <param name="value">Token symbol/string/value.</param>
		public void NewToken(string value)
		{
			Items.Add(new Token(this, value));
		}

		/// <summary>Comments located at the end of the scope.</summary>
		public List<string> EndComments;

		/// <summary>Child members.</summary>
		public readonly List<Construct> Items;
	}
}
