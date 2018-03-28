#region Include
using System.Collections.Generic;
#endregion

namespace Clausewitz.Constructs
{
	/// <summary>A clause with a child members in Clausewitz.</summary>
	public class Scope : Construct
	{
		#region Constructors
		/// <summary>Primary constructor.</summary>
		/// <param name="scope">Parent scope.</param>
		/// <param name="name">Optional name.</param>
		/// <param name="items">Optional items.</param>
		internal Scope(Scope scope, string name = null, List<Construct> items = null) : base(Types.Scope, scope)
		{
			if (name != null)
				Name = name;
			if (items != null)
			{
				Items = items;
				foreach (var item in items)
					item.Scope = this;
			}
			else
			{
				Items = new List<Construct>();
			}
		}
		#endregion

		#region Methods
		/// <summary>Creates a new binding within this scope.</summary>
		/// <param name="name">Left side.</param>
		/// <param name="value">Right side.</param>
		public void NewBinding(string name, string value)
		{
			Items.Add(new Binding(this, name, value));
		}
		/// <summary>Creates a new scope within this scope.</summary>
		/// <param name="name">Optional name.</param>
		/// <param name="items">Optional items.</param>
		public void NewScope(string name = null, List<Construct> items = null)
		{
			Items.Add(new Scope(this, name, items));
		}
		/// <summary>Creates a new token within this scope.</summary>
		/// <param name="value">Token symbol/string/value.</param>
		public void NewToken(string value)
		{
			Items.Add(new Token(this, value));
		}
		#endregion

		#region Fields
		/// <summary>Comments located at the end of the scope.</summary>
		public List<string> EndComments;

		/// <summary>Child members.</summary>
		public readonly List<Construct> Items;

		/// <summary>Optional scope name (not all scopes have names in Clausewitz)</summary>
		public string Name;
		#endregion
	}
}
