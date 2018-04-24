﻿using System.Collections.Generic;
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
		private Scope(Scope parent, string name = null) : base(parent)
		{
			if (name != null)
				Name = name;
		}

		/// <summary>Optional scope name (not all scopes have names in Clausewitz)</summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>Creates a new binding within this scope. (Automatically assigns the parent)</summary>
		/// <param name="name">Left side.</param>
		/// <param name="value">Right side.</param>
		/// <returns>New binding.</returns>
		public Binding NewBinding(string name, string value)
		{
			var binding = new Binding(this, name, value);
			Members.Add(binding);
			return binding;
		}

		/// <summary>Creates a new scope within this scope. (Automatically assigns the parent)</summary>
		/// <param name="name">Optional name.</param>
		/// <returns>New scope.</returns>
		public Scope NewScope(string name = null)
		{
			var scope = new Scope(this, name);
			Members.Add(scope);
			return scope;
		}

		/// <summary>Creates a new token within this scope. (Automatically assigns the parent)</summary>
		/// <param name="value">Token symbol/string/value.</param>
		/// <returns>New token.</returns>
		public Token NewToken(string value)
		{
			var token = new Token(this, value);
			Members.Add(token);
			return token;
		}

		/// <summary>Comments located at the end of the scope.</summary>
		public List<string> EndComments;

		/// <summary>Child members.</summary>
		public readonly List<Construct> Members = new List<Construct>();
	}
}
