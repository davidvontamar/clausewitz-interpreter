using System;

namespace Tamar.Clausewitz.Constructs;

/// <summary>
/// Any statement which includes the assignment operator '=' in the Clausewitz language, except clauses.
/// Including most commands, conditions and triggers which come in a single line.
/// </summary>
public class Binding : Construct
{
    public Binding(Clause parent, string name, string value) : base(parent)
    {
        if (!Interpreter.IsValidToken(name))
            throw new Exception("Invalid name.");
        if (!Interpreter.IsValidToken(value))
            throw new Exception("Invalid value.");
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Special base constructor for typed bindings.
    /// </summary>
    protected Binding(Clause parent, string name) : base(parent)
    {
        if (!Interpreter.IsValidToken(name))
            throw new Exception("Invalid name.");
        Name = name;
    }

    /// <summary>Left side.</summary>
    public string Name;

    /// <summary>Right side.</summary>
    public string Value;
}