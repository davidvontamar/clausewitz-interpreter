using System.Collections.Generic;
namespace Clausewitz.Constructs
{
	/// <summary>Basic Clausewitz language construct.</summary>
	public abstract class Construct
	{
		/// <summary>Construct type & parent must be defined when created.</summary>
		/// <param name="type">Construct type.</param>
		/// <param name="parent">Parent scope.</param>
		protected Construct(Types type, Scope parent)
		{
			Type = type;
			Parent = parent;
		}

		/// <summary>Scope depth level within file.</summary>
		public int Level
		{
			get
			{
				// This recursive function retrieves the count of all parents up to the root.
				var parentScopes = 0;
				var currentScope = Parent;
				while (true)
				{
					if (currentScope == null)
						return parentScopes;
					parentScopes++;
					currentScope = Parent.Parent;
				}
			}
		}

		/// <summary>Associated comments.</summary>
		public List<string> Comments;

		/// <summary>The parent scope.</summary>
		public Scope Parent;

		/// <summary>Construct type.</summary>
		public readonly Types Type;

		/// <summary>All language constructs found in Clausewitz's syntax.</summary>
		public enum Types
		{
			Scope,
			Token,
			Binding
		}
	}
}
