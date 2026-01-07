using System;

namespace Tamar.Clausewitz.Constructs;

/// <summary>A single token, sometimes a number, a string or just a symbol.</summary>
public class Token : Construct
{
    /// <summary>Primary constructor.</summary>
    /// <param name="parent">Parent clause.</param>
    /// <param name="value">The token itself.</param>
    internal Token(Clause parent, string value) : base(parent)
    {
        Value = value;
    }

    /// <summary>The token itself</summary>
    public string Value
    {
        get;
        set
        {
            if (!Interpreter.IsValidToken(value))
                throw new ArgumentException($"The string '{value}' is not a valid token.");
            field = value;
        }
    }
}