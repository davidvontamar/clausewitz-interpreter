using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tamar.Clausewitz.Constructs;

namespace Tamar.Clausewitz;

/// <summary>The Clausewitz interpreter</summary>
public static class Interpreter
{
    internal const string ValuePattern = @"[a-zA-Z0-9_.:""]+";

    internal static bool IsValidToken(string token)
    {
        return Regex.IsMatch(token, @"\d") || token == "---" || Regex.IsMatch(token, ValuePattern);
    }

    internal static List<(string token, int line)> Tokenize(string text)
    {
        // The current token so far recorded since the last token-breaking character.
        var currentToken = string.Empty;

        // All tokenized tokens within this file so far.
        var tokens = new List<(string token, int line)>();

        // Indicates a delimited string token.
        var isString = false;

        // Indicates a delimited comment token.
        var isComment = false;

        // Indicates a new line.
        var newline = false;

        // Counts each newline.
        var lineNumber = 1;

        // Keeps track of the previous char.
        var previousChar = '\0';

        // Tokenization loop:
        foreach (var @char in text)
        {
            // Count a new line.
            if (newline)
            {
                lineNumber++;
                newline = false;
            }

            // Keep tokenizing a string unless a switching delimiter comes outside escape.
            if (isString && !(@char == '"' && previousChar != '\\'))
                goto concat;

            // Keep tokenizing a comment unless a switching delimiter comes.
            if (isComment && !(@char == '\r' || @char == '\n'))
                goto concat;

            // Standard tokenizer:
            var charToken = '\0';
            switch (@char)
            {
                // Newline: (also comment delimiter)
                case '\r':
                case '\n':

                    // Switch comments:
                    if (isComment)
                    {
                        isComment = false;

                        // Add empty comments:
                        if (currentToken.Length == 0)
                            tokens.Add((string.Empty, lineNumber));
                    }

                    // Cross-platform compatibility for newlines:
                    if (previousChar == '\r' && @char == '\n')
                        break;
                    newline = true;
                    break;

                // Whitespace (which breaks tokens):
                case ' ':
                case '\t':
                    break;

                // String delimiter:
                case '"':
                    isString = !isString;
                    currentToken += @char;
                    break;

                // Comment delimiter:
                case '#':
                    isComment = true;
                    charToken = @char;
                    break;

                // Clause clauses & binding operator:
                case '}':
                case '{':
                case '=':
                    charToken = @char;
                    break;

                // Any other character:
                default:
                    goto concat;
            }

            // Add new tokens to the list:
            if (currentToken.Length > 0 && !isString)
            {
                tokens.Add((currentToken, lineNumber));
                currentToken = string.Empty;
            }

            if (charToken != '\0')
                tokens.Add((new string(charToken, 1), lineNumber));
            previousChar = @char;
            continue;

        // Concat characters to unfinished numbers/words/comments/strings.
        concat:
            currentToken += @char;
            previousChar = @char;
        }

        // EOF & last token:
        if (currentToken.Length > 0 && !isString)
            tokens.Add((currentToken, lineNumber));
        return tokens;
    }
    /// <summary>
    /// Translate a Clause structure back into Clausewitz script.
    /// </summary>
    /// <param name="root">Root clause</param>
    /// <returns>Clausewitz script</returns>
    public static string TranslateClause(Clause root)
    {
        var data = new List<string>();
        var newline = Environment.NewLine;
        var tabs = new string('\t', root.Level);

        // Files include their own comments at the beginning followed by an empty line.
        if (root.Parent == null)
            if (root.Comments.Count > 0)
            {
                foreach (var comment in root.Comments)
                    data.Add(tabs + "# " + comment + newline);
                data.Add(newline);
            }

        // Translate clause members:
        foreach (var construct in root.Constructs)
        {
            foreach (var comment in construct.Comments)
                data.Add(tabs + "# " + comment + newline);

            // Translate the actual type:
            switch (construct)
            {
                case Clause clause:
                    if (string.IsNullOrWhiteSpace(clause.Name))
                        data.Add(tabs + '{');
                    else
                        data.Add(tabs + EnquoteIfRequired(clause.Name) + " = {");
                    if (clause.Constructs.Count > 0)
                    {
                        data.Add(newline + TranslateClause(clause));
                        foreach (var comment in clause.EndComments)
                            data.Add(tabs + "\t" + "# " + comment + newline);
                        data.Add(tabs + '}' + newline);
                    }
                    else
                    {
                        data.Add('}' + newline);
                    }

                    break;
                case Binding binding:
                    data.Add(tabs + EnquoteIfRequired(binding.Name) + " = " + EnquoteIfRequired(binding.Value) + newline);
                    break;
                case Token token:
                    if (root.IsIndented)
                    {
                        data.Add(tabs + EnquoteIfRequired(token.Value) + newline);
                    }
                    else
                    {
                        var preceding = " ";
                        var following = string.Empty;

                        // Preceding characters:
                        if (root.Constructs.First() == token)
                            preceding = tabs;
                        else if (token.Comments.Count > 0)
                            preceding = tabs;
                        else if (root.Constructs.First() != token)
                            if (root.Constructs[root.Constructs.IndexOf(token) - 1] is not Token)
                                preceding = tabs;

                        // Following characters:
                        if (root.Constructs.Last() != token)
                        {
                            var next = root.Constructs[root.Constructs.IndexOf(token) + 1];
                            if (next is not Token)
                                following = newline;
                            if (next.Comments.Count > 0)
                                following = newline;
                        }
                        else if (root.Constructs.Last() == token)
                        {
                            following = newline;
                        }

                        data.Add(preceding + EnquoteIfRequired(token.Value) + following);
                    }
                    break;
            }
        }

        // Append end comments in files:
        if (root.Parent == null)
            foreach (var comment in root.EndComments)
                data.Add(newline + "# " + comment);
        return string.Concat(data);
    }

    private static string Unquote(string text)
    {
        if (text.First() == '"' || text.Last() == '"')
            return text.Remove(text.Length - 1, 1).Remove(0, 1);
        return text;
    }

    private static string EnquoteIfRequired(string text)
    {
        if (text.Contains(' ') && (text.First() != '"' || text.Last() != '"'))
            return '"' + text + '"';
        if (text.Length == 0)
            return "\"\"";
        return text;
    }
    public static Clause InterpretText(string text)
    {
        // Tokenize the file:
        var tokens = Tokenize(text);

        // All associated comments so far.
        var comments = new List<(string text, int line)>();

        // Root clause.
        var rootClause = new Clause(null);

        // Current clause.
        var currentClause = rootClause;

        // Interpretation loop:
        for (var index = 0; index < tokens.Count; index++)
        {
            // All current information:
            var token = tokens[index].token;
            var nextToken = index < tokens.Count - 1 ? tokens[index + 1].token : string.Empty;
            var prevToken = index > 0 ? tokens[index - 1].token : string.Empty;
            var prevPrevToken = index > 1 ? tokens[index - 2].token : string.Empty;
            var lineNumber = tokens[index].line;

            // Interpret tokens:
            // Enter a new clause:
            if (token == "{" && prevToken != "#")
            {
                // Participants:
                var name = prevPrevToken;
                var binding = prevToken;

                // Syntax check:
                if (binding == "=")
                {
                    if (IsValidToken(name))
                        currentClause = currentClause.AddClause(name);
                    else
                        throw new SyntaxException("Invalid name in clause binding.", currentClause, lineNumber, token);
                }
                else
                {
                    currentClause = currentClause.AddClause();
                }

                AssociateComments(currentClause);
            }
            // Exit the current clause:
            else if (token == "}" && prevToken != "#")
            {
                // Associate end comments:
                AssociateComments(currentClause, true);

                // Check if the current clause is the file, if so, then notify an error of a missing opening brace "{".
                if (currentClause.Parent == null)
                {
                    throw new SyntaxException("Missing an opening brace '{' for a clause", currentClause, lineNumber,
                        token);
                }
                else
                {
                    if (currentClause.IsSorted)
                        currentClause.Sort();
                    currentClause = currentClause.Parent;
                }
            }
            // Binding operator:
            else if (token == "=" && prevToken != "#")
            {
                // Participants:
                var name = prevToken;
                var value = nextToken;

                // Skip clause binding: (handled in the "{" case, otherwise will claim as a syntax error.)
                if (value == "{")
                    continue;

                // Syntax check:
                if (!IsValidToken(name))
                    throw new SyntaxException("Invalid name in binding.", currentClause, lineNumber, token);
                if (!IsValidToken(value))
                    throw new SyntaxException("Invalid value in binding.", currentClause, lineNumber, token);
                currentClause.AddBinding(Unquote(name), Unquote(value));
                AssociateComments();
            }
            // Comment/pragma:
            else if (token == "#")
            {
                // An attached comment is a comment that comes at the same line with another language construct:
                // If the comment comes at the same line with another construct, then it will be associated with that construct.
                // If the comment takes a whole line then it will be stacked and associated with the next construct.
                // If there was an empty line after the comment at the beginning of the file, then it will be associated with the root clause instead.
                // Comments are also responsible for pragmas in this interpreter.
                var lineOfPreviousToken = index > 0 ? tokens[index - 1].line : -1;
                var isAttached = lineNumber == lineOfPreviousToken;

                // Associate attached comments here:
                if (isAttached)
                {
                    if (prevToken != "{")
                        currentClause.Constructs.Last().Comments.Add(nextToken.Trim());
                    else
                        currentClause.Comments.Add(nextToken.Trim());
                }
                else
                {
                    comments.Add((nextToken.Trim(), lineNumber));
                }
            }
            // Unattached value/word token:
            else
            {
                // Check if bound:
                var isBound = prevToken.Contains('=') || nextToken.Contains('=');

                // Check if commented:
                var isComment = prevToken.Contains('#');

                // Skip those cases:
                if (!isBound && !isComment)
                {
                    if (IsValidToken(token))
                    {
                        currentClause.AddToken(Unquote(token));
                        AssociateComments();
                    }
                    else
                    {
                        throw new SyntaxException("Unexpected token.", currentClause, lineNumber, token);
                    }
                }
            }
        }

        // Missing a closing brace "{" for clauses above the root level:
        if (currentClause != rootClause)
            throw new SyntaxException("Missing a closing '}' for a clause.", currentClause, tokens.Last().line,
                tokens.Last().token);

        // Associate end-comments (of the file):
        AssociateComments(currentClause, true);

        // Associate the stacking comments with the latest language construct.
        void AssociateComments(Construct construct = null, bool endComments = false)
        {
            // No comments, exit.
            if (comments.Count == 0)
                return;

            // Associate with last construct if parameter is null.
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (construct == null && currentClause.Constructs.Count == 0)
                return;
            if (construct == null)
                construct = currentClause.Constructs.Last();

            // Leading comments at the beginning of a file:
            if (!endComments && construct.Parent.Parent == null && construct.Parent.Constructs.First() == construct)
            {
                var associatedWithFile = new List<string>();
                var associatedWithConstruct = new List<string>();
                var associateWithFile = false;

                // Reverse iteration:
                for (var index = comments.Count - 1; index >= 0; index--)
                {
                    if (!associateWithFile)
                    {
                        var prevCommentLine = index < comments.Count - 1 ? comments[index + 1].line : -1;
                        var commentLine = comments[index].line;
                        if (prevCommentLine > 1 && prevCommentLine - commentLine != 1)
                            associateWithFile = true;
                    }

                    if (associateWithFile)
                        associatedWithFile.Add(comments[index].text);
                    else
                        associatedWithConstruct.Add(comments[index].text);
                }

                // Reverse & append:
                construct.Parent.Comments.AddRange(associatedWithFile.Reverse<string>());
                construct.Comments.AddRange(associatedWithConstruct.Reverse<string>());
            }
            else if (!endComments)
            {
                foreach (var comment in comments)
                    construct.Comments.Add(comment.text);
            }

            else if (construct is Clause commentClause)
            {
                foreach (var comment in comments)
                    commentClause.EndComments.Add(comment.text);
            }

            comments.Clear();
        }

        return rootClause;
    }
    public class SyntaxException : Exception
    {
        /// <summary>The file where the exception occurred.</summary>
        public readonly Clause FileClause;

        /// <summary>The line at which the exception occurred.</summary>
        public readonly int Line;

        /// <summary>The token responsible for the exception.</summary>
        public readonly string Token;
        internal SyntaxException(string message, Clause fileClause, int line, string token) : base(message)
        {
            FileClause = fileClause;
            Line = line;
            Token = token;
        }
        public string Details =>
            $"Token: '{Token}'\nLine: {Line}\nFile: {FileClause.Name}";
    }
}