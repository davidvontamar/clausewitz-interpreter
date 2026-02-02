using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tamar.Clausewitz.Constructs;

public class Clause : Construct
{
    protected internal Clause(Clause parent, string name = null) : base(parent)
    {
        if (name != null)
            Name = name;
        Constructs = new ReadOnlyCollection<Construct>(constructs);
        Bindings = new ReadOnlyCollection<Binding>(bindings);
        Clauses = new ReadOnlyCollection<Clause>(clauses);
        Tokens = new ReadOnlyCollection<Token>(tokens);
    }
    /// <summary>
    /// Special constructor for root clauses without a parent
    /// </summary>
    protected Clause(string name) : this(null, name)
    {
    }
    public readonly ReadOnlyCollection<Binding> Bindings;
    public readonly List<string> EndComments = [];
    public readonly ReadOnlyCollection<Construct> Constructs;
    public readonly ReadOnlyCollection<Clause> Clauses;
    public readonly ReadOnlyCollection<Token> Tokens;
    /// <summary>
    /// All child constructs; for internal uses by the interpreter
    /// </summary>
    private readonly List<Construct> constructs = [];
    private readonly List<Binding> bindings = [];
    private readonly List<Clause> clauses = [];
    private readonly List<Token> tokens = [];
    public bool IsIndented
    {
        get
        {
            if (Pragmas.ContainsOptions("indent"))
                return true;
            if (Pragmas.ContainsOptions("unindent"))
                return false;
            if (IsIndentedParent(Parent))
                return true;
            if (IsUnindentedParent(Parent))
                return false;
            if (HasOnlyTokens() && constructs.Count > 20)
                return false;
            return true;
            bool IsIndentedParent(Clause parent)
            {
                while (true)
                {
                    if (parent == null)
                        return false;
                    if (parent.Pragmas.ContainsOptions("indent", "all"))
                        return true;
                    parent = parent.Parent;
                }
            }
            bool IsUnindentedParent(Clause parent)
            {
                while (true)
                {
                    if (parent == null)
                        return false;
                    if (parent.Pragmas.ContainsOptions("unindent", "all"))
                        return true;
                    parent = parent.Parent;
                }
            }
            bool HasOnlyTokens()
            {
                foreach (var member in constructs)
                    if (member is not Token)
                        return false;
                return true;
            }
        }
    }
    public bool IsSorted
    {
        get
        {
            return HasSortedParent(Parent) || Pragmas.ContainsOptions("sort");

            /// <summary>
            /// Checks if any parent clause has sorting enabled.
            /// </summary>
            static bool HasSortedParent(Clause parent)
            {
                while (true)
                {
                    if (parent == null)
                        return false;
                    if (parent.Pragmas.ContainsOptions("sort", "all"))
                        return true;
                    parent = parent.Parent;
                }
            }
        }
    }
    public string Name
    {
        get; set
        {
            if (!Interpreter.IsValidToken(value))
                throw new ArgumentException("Invalid Clausewitz token.", nameof(value));
            field = value;
        }
    }
    public void CopyAllConstructsFrom(Clause source)
    {
        foreach (var member in source.constructs)
            if (member is Binding binding)
            {
                var newBinding = AddBinding(binding.Name, binding.Value);
                newBinding.Comments.AddRange(binding.Comments);
            }
            else if (member is Clause scope)
            {
                var newScope = AddClause(scope.Name);
                newScope.CopyAllConstructsFrom(scope);
                newScope.Comments.AddRange(scope.Comments);
            }
            else if (member is Token token)
            {
                var newToken = AddToken(token.Value);
                newToken.Comments.AddRange(token.Comments);
            }
    }
    public Binding FindBinding(string name)
    {
        foreach (var member in constructs)
            if (member is Binding binding && binding.Name == name)
                return binding;
        return null;
    }
    public Clause FindClauseDepthFirst(string name)
    {
        var stack = new Stack<Clause>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.Name == name)
                return current;
            foreach (var clause in current.Clauses)
            {
                stack.Push(clause);
            }
        }
        return null;
    }
    public Clause FindClauseBreadthFirst(string name)
    {
        var queue = new Queue<Clause>();
        queue.Enqueue(this);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.Name == name)
                return current;
            foreach (var clause in current.Clauses)
            {
                queue.Enqueue(clause);
            }
        }
        return null;
    }
    public bool HasToken(string value)
    {
        foreach (var member in constructs)
            if (member is Token token && token.Value == value)
                return true;
        return false;
    }
    public Binding AddBinding(string name, string value)
    {
        var binding = new Binding(this, name, value);
        bindings.Add(binding);
        constructs.Add(binding);
        return binding;
    }
    public Clause AddClause(string name = null)
    {
        var scope = new Clause(this, name);
        clauses.Add(scope);
        constructs.Add(scope);
        return scope;
    }
    public Token AddToken(string value)
    {
        var token = new Token(this, value);
        tokens.Add(token);
        constructs.Add(token);
        return token;
    }
    public void RemoveBinding(Binding binding)
    {
        if (binding.Parent != this)
            throw new ArgumentException("The binding to remove is not a child of this clause.", nameof(binding));
        bindings.Remove(binding);
        constructs.Remove(binding);
    }
    public void RemoveClause(Clause clause)
    {
        if (clause.Parent != this)
            throw new ArgumentException("The clause to remove is not a child of this clause.", nameof(clause));
        clauses.Remove(clause);
        constructs.Remove(clause);
    }
    public void RemoveToken(Token token)
    {
        if (token.Parent != this)
            throw new ArgumentException("The token to remove is not a child of this clause.", nameof(token));
        tokens.Remove(token);
        constructs.Remove(token);
    }
    public void Sort()
    {
        constructs.Sort((first, second) =>
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
                    case Clause scope:
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
}