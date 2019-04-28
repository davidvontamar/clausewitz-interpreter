using System.Collections.Generic;

namespace Tamar.Clausewitz.Constructs
{
	/// <summary>Scopes are files, direcotries, and clauses.</summary>
	public class Scope: Construct
	{
		/// <summary>
		/// Special scope constructor for files (which has no parent scope, but parent
		/// directory).
		/// </summary>
		/// <param name="name">Scope name.</param>
		protected Scope(string name): this(null, name)
		{
			// implemented at primary constructor.
		}

		/// <summary>Primary constructor.</summary>
		/// <param name="parent">Parent scope.</param>
		/// <param name="name">Optional name.</param>
		private Scope(Scope parent, string name = null): base(parent)
		{
			if (name != null)
				Name = name;
		}

		/// <summary>If false, all members within this scope will come in a single line.</summary>
		public bool Indented
		{
			get
			{
				if (Pragmas.Contains("indent"))
					return true;
				if (Pragmas.Contains("unindent"))
					return false;
				if (IndentedParent(Parent))
					return true;
				if (UnindentedParent(Parent))
					return false;
				if (AllTokens() && (Members.Count > 20))
					return false;
				return true;
				bool IndentedParent(Scope parent)
				{
					while (true)
					{
						if (parent == null)
							return false;
						if (parent.Pragmas.Contains("indent", "all"))
							return true;
						parent = parent.Parent;
					}
				}
				bool UnindentedParent(Scope parent)
				{
					while (true)
					{
						if (parent == null)
							return false;
						if (parent.Pragmas.Contains("unindent", "all"))
							return true;
						parent = parent.Parent;
					}
				}
				bool AllTokens()
				{
					foreach (var member in Members)
						if (!(member is Token))
							return false;
					return true;
				}
			}
		}

		/// <summary>Optional scope name (not all scopes have names in Clausewitz)</summary>
		public string Name { get; set; }

		/// <summary>If true, all members within this scope will be sorted alphabetically.</summary>
		public bool Sorted
		{
			get
			{
				return SortedParent(Parent) || Pragmas.Contains("sort");
				bool SortedParent(Scope parent)
				{
					while (true)
					{
						if (parent == null)
							return false;
						if (parent.Pragmas.Contains("sort", "all"))
							return true;
						parent = parent.Parent;
					}
				}
			}
		}

		/// <summary>
		/// Creates a new binding within this scope. (Automatically assigns the
		/// parent)
		/// </summary>
		/// <param name="name">Left side.</param>
		/// <param name="value">Right side.</param>
		/// <returns>New binding.</returns>
		public Binding NewBinding(string name, string value)
		{
			var binding = new Binding(this, name, value);
			Members.Add(binding);
			return binding;
		}

		/// <summary>
		/// Creates a new scope within this scope. (Automatically assigns the
		/// parent)
		/// </summary>
		/// <param name="name">Optional name.</param>
		/// <returns>New scope.</returns>
		public Scope NewScope(string name = null)
		{
			var scope = new Scope(this, name);
			Members.Add(scope);
			return scope;
		}

		/// <summary>
		/// Creates a new token within this scope. (Automatically assigns the
		/// parent)
		/// </summary>
		/// <param name="value">Token symbol/string/value.</param>
		/// <returns>New token.</returns>
		public Token NewToken(string value)
		{
			var token = new Token(this, value);
			Members.Add(token);
			return token;
		}

		/// <summary>
		/// Sorts members alphabetically.
		/// </summary>
		public void Sort()
		{
			Members.Sort((first, second) =>
			{
				var constructs = new[]
				{
					first,
					second
				};
				var values = new string[2];
				for (var index = 0; index < constructs.Length; index++)
				{
					var construct = constructs[index];
					switch (construct)
					{
						case Binding binding:
							values[index] = binding.Name;
							break;
						case Scope scope:
							values[index] = scope.Name;
							break;
						case Token token:
							values[index] = token.Value;
							break;
					}
				}
				return string.CompareOrdinal(values[0], values[1]);
			});
		}

		/// <summary>Comments located at the end of the scope.</summary>
		public List<string> EndComments = new List<string>();

		/// <summary>Child members.</summary>
		public readonly List<Construct> Members = new List<Construct>();
	}
}