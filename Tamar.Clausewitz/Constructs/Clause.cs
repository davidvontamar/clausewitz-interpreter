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
        Members = new ReadOnlyCollection<Construct>(Constructs);
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
    public readonly ReadOnlyCollection<Construct> Members;
    public readonly ReadOnlyCollection<Clause> Clauses;
    public readonly ReadOnlyCollection<Token> Tokens;
    /// <summary>
    /// All child constructs; for internal uses by the interpreter
    /// </summary>
    internal readonly List<Construct> Constructs = [];
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
            if (IndentedParent(Parent))
                return true;
            if (UnindentedParent(Parent))
                return false;
            if (AllTokens() && Constructs.Count > 20)
                return false;
            return true;
            bool IndentedParent(Clause parent)
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
            bool UnindentedParent(Clause parent)
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
            bool AllTokens()
            {
                foreach (var member in Constructs)
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
            return SortedParent(Parent) || Pragmas.ContainsOptions("sort");

            bool SortedParent(Clause parent)
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
        foreach (var member in source.Constructs)
            if (member is Binding binding)
            {
                var newBinding = AddNewBinding(binding.Name, binding.Value);
                newBinding.Comments.AddRange(binding.Comments);
            }
            else if (member is Clause scope)
            {
                var newScope = AddNewClause(scope.Name);
                newScope.CopyAllConstructsFrom(scope);
                newScope.Comments.AddRange(scope.Comments);
            }
            else if (member is Token token)
            {
                var newToken = AddNewToken(token.Value);
                newToken.Comments.AddRange(token.Comments);
            }
    }
    public Binding FindBinding(string name)
    {
        foreach (var member in Constructs)
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
        foreach (var member in Constructs)
            if (member is Token token && token.Value == value)
                return true;
        return false;
    }
    public Binding AddNewBinding(string name, string value)
    {
        var binding = new Binding(this, name, value);
        bindings.Add(binding);
        Constructs.Add(binding);
        return binding;
    }
    public Clause AddNewClause(string name = null)
    {
        var scope = new Clause(this, name);
        clauses.Add(scope);
        Constructs.Add(scope);
        return scope;
    }
    public Token AddNewToken(string value)
    {
        var token = new Token(this, value);
        tokens.Add(token);
        Constructs.Add(token);
        return token;
    }
    public void Sort()
    {
        Constructs.Sort((first, second) =>
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