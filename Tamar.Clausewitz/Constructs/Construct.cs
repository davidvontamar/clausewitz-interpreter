using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tamar.Clausewitz.Constructs;

/// <summary>Basic Clausewitz language construct.</summary>
public abstract class Construct
{
    /// <summary>Construct type & parent must be defined when created.</summary>
    /// <param name="parent">Parent clause.</param>
    protected Construct(Clause parent)
    {
        Parent = parent;
    }

    /// <summary>Associated comments.</summary>
    public readonly List<string> Comments = new();

    /// <summary>Clause's depth level within the containing file.</summary>
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
                currentScope = currentScope.Parent;
            }
        }
    }

    /// <summary>The parent clause.</summary>
    public Clause Parent
    {
        get; internal set;
    }

    /// <summary>
    /// Extracts pragmas from associated comments within brackets, which are separated by commas; 
    /// and their options, which are separated by spaces.
    /// </summary>
    public IEnumerable<Pragma> Pragmas
    {
        get
        {
            var allComments = new List<string>();
            var @return = new HashSet<Pragma>();
            if (Comments != null)
                allComments.AddRange(Comments);
            if (this is Clause scope)
                if (scope.EndComments != null)
                    allComments.AddRange(scope.EndComments);
            if (allComments.Count == 0)
                return @return;
            foreach (var comment in allComments)
            {
                if (!(comment.Contains('[') && comment.Contains(']')))
                    continue;

                // All pragmas are guaranteed to be lower case and trimmed.
                var pragmas = Regex.Replace(comment.Split('[', ']')[1], @"\s+", " ").ToLower().Split(',');
                foreach (var pragma in pragmas)
                    @return.Add(new Pragma(pragma.Split(' ')));
            }

            return @return;
        }
    }
}