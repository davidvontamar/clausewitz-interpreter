using System;
using System.IO;
using System.Linq;
using Tamar.Clausewitz.Constructs;

namespace Tamar.Clausewitz.Test;

/// <summary>
/// Basic command-line interface for the Clausewitz interpreter.
/// </summary>
public static class Program
{
    private static readonly ConsoleColor BindingForegroundColor = ConsoleColor.Magenta;
    private static readonly ConsoleColor CommentForegroundColor = ConsoleColor.Green;
    private static readonly ConsoleColor RootBackgroundColor = ConsoleColor.DarkCyan;
    private static readonly ConsoleColor RootForegroundColor = ConsoleColor.Cyan;
    private static readonly ConsoleColor ClauseBackgroundColor = ConsoleColor.Blue;
    private static readonly ConsoleColor ClauseForegroundColor = ConsoleColor.Cyan;
    private static readonly ConsoleColor TokenBackgroundColor = ConsoleColor.DarkBlue;
    private static readonly ConsoleColor TokenForegroundColor = ConsoleColor.White;
    private static readonly ConsoleColor TreeForegroundColor = ConsoleColor.DarkGray;

    /// <summary>Main entry point.</summary>
    public static void Main()
    {
        Console.WriteLine("This is an interpreter for the Clausewitz language, written by David von Tamar and released under the LGPL-3.0 license.");
        var path = "TestFiles/Example.txt";
        var text = File.ReadAllText(path);
        var root = Interpreter.InterpretText(text);
        root.Name = Path.GetFileName(path);
        PrettyPrint(root);
        Console.WriteLine("Translation:");
        Console.WriteLine(Interpreter.TranslateClause(root));
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    /// <summary>Draws tree structure to the left.</summary>
    /// <param name="root">
    /// The initial node. (will stop looking for parents beyond this member)
    /// </param>
    /// <param name="current">Current construct.</param>
    /// <param name="alignment">
    /// Whether this line opens a new node from its parent scope, or rather drawn above
    /// or beneath a node.
    /// </param>
    private static string ConcatTree(Construct root, Construct current, Alignment alignment = Alignment.Inner)
    {
        if (root == current)
            return string.Empty;
        var tree = string.Empty;
        var isLast = false;
        var parent = current.Parent;
        if (parent.Constructs.Last() == current)
            isLast = true;
        tree += ConcatTree(root, parent);
        switch (alignment)
        {
            case Alignment.Before:
                tree += "│ ";
                break;
            case Alignment.After:
            case Alignment.Inner:
                {
                    if (isLast)
                        tree += "  ";
                    else
                        tree += "│ ";
                    break;
                }
            case Alignment.Node when isLast:
                tree += "└─";
                break;
            case Alignment.Node:
                tree += "├─";
                break;
        }
        return tree;
    }

    private static void PrettyPrint(Construct root, Construct current = null)
    {
        if (current == null)
            current = root;
        if (current is Construct construct)
            foreach (var comment in construct.Comments)
            {
                Console.ForegroundColor = TreeForegroundColor;
                Console.Write(ConcatTree(root, construct, Alignment.Before));
                Console.ResetColor();
                Console.ForegroundColor = CommentForegroundColor;
                Console.Write(comment);
                Console.ResetColor();
                Console.WriteLine();
            }
        Console.ForegroundColor = TreeForegroundColor;
        Console.Write(ConcatTree(root, current, Alignment.Node));
        Console.ResetColor();
        switch (current)
        {
            case Binding binding:
                Console.ForegroundColor = TokenForegroundColor;
                Console.BackgroundColor = TokenBackgroundColor;
                Console.Write(binding.Name);
                Console.ResetColor();
                Console.ForegroundColor = BindingForegroundColor;
                Console.Write(" = ", BindingForegroundColor);
                Console.ResetColor();
                Console.ForegroundColor = TokenForegroundColor;
                Console.BackgroundColor = TokenBackgroundColor;
                Console.Write(binding.Value);
                Console.ResetColor();
                Console.WriteLine();
                break;
            case Clause clause:
                if (clause.Parent == null)
                {
                    Console.ForegroundColor = RootForegroundColor;
                    Console.BackgroundColor = RootBackgroundColor;
                    Console.Write(clause.Name);
                    Console.ResetColor();
                    Console.WriteLine();
                }
                else if (!string.IsNullOrWhiteSpace(clause.Name))
                {
                    Console.ForegroundColor = ClauseForegroundColor;
                    Console.BackgroundColor = ClauseBackgroundColor;
                    Console.Write(clause.Name);
                    Console.ResetColor();
                    Console.WriteLine();
                }
                else
                {
                    Console.ForegroundColor = TreeForegroundColor;
                    Console.Write(clause.Constructs.Count > 0 ? "┐" : "─");
                    Console.ResetColor();
                    Console.WriteLine();
                }
                foreach (var member in clause.Constructs)
                    PrettyPrint(root, member);
                foreach (var comment in clause.EndComments)
                {
                    Console.ForegroundColor = TreeForegroundColor;
                    Console.Write(ConcatTree(root, clause, Alignment.After));
                    Console.ResetColor();
                    Console.ForegroundColor = CommentForegroundColor;
                    Console.Write("  " + comment);
                    Console.ResetColor();
                    Console.WriteLine();
                }
                break;
            case Token token:
                Console.ForegroundColor = TokenForegroundColor;
                Console.BackgroundColor = TokenBackgroundColor;
                Console.Write(token.Value);
                Console.ResetColor();
                Console.WriteLine();
                break;
        }

        Console.ResetColor();
    }

    /// <summary>
    /// Determines the junctions at the tree hierarchy in pretty-printing.
    /// </summary>
    private enum Alignment
    {
        Before,
        Node,
        After,
        Inner
    }
}