#region Include
using System.Collections.Generic;
#endregion

namespace Clausewitz.Constructs
{
	/// <summary>Basic Clausewitz language construct.</summary>
	public abstract class Construct
	{
		#region Constructors
		/// <summary>Construct type & parent must be defined when created.</summary>
		/// <param name="type">Construct type.</param>
		/// <param name="scope">Parent scope.</param>
		protected Construct(Types type, Scope scope)
		{
			Type = type;
			Scope = scope;
		}
		#endregion

		#region Properties
		/// <summary>Depth level from Root construct.</summary>
		public int Level
		{
			get
			{
				// This recursive function retrieves the count of all parents up to the root.
				var parentScopes = 0;
				var currentScope = Scope;
				while (true)
				{
					if (currentScope == null)
						return parentScopes;
					parentScopes++;
					currentScope = Scope.Scope;
				}
			}
		}
		#endregion

		#region Fields
		/// <summary>Associated comments.</summary>
		public List<string> Comments;

		/// <summary>The parent scope.</summary>
		public Scope Scope = null;

		/// <summary>Construct type.</summary>
		public readonly Types Type;
		#endregion

		#region Nested types
		/// <summary>All language constructs found in Clausewitz's syntax.</summary>
		public enum Types
		{
			Scope,
			Token,
			Binding
		}
		#endregion
	}
}
