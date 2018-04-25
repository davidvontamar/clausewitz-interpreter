using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using Clausewitz.Constructs;
using Clausewitz.IO;
using Directory = Clausewitz.IO.Directory;
using File = Clausewitz.IO.File;
namespace Clausewitz
{
	/// <summary>The Clausewitz interpreter.</summary>
	public static class Interpreter
	{
		/// <summary>
		/// Regex rule for valid Clausewitz values. Includes: identifiers, numerical values, and ':' variable binding operator.
		/// </summary>
		internal const string ValueRegexRule = @"[a-zA-Z0-9_.:""]+";

		/// <summary>
		/// Checks if a token is a valid value in Clausewitz syntax standards for both names & values.
		/// </summary>
		/// <param name="token">Token.</param>
		/// <returns>Boolean.</returns>
		internal static bool IsValidValue(string token)
		{
			return Regex.IsMatch(token, @"\d") || Regex.IsMatch(token, ValueRegexRule);
		}
		
		/// <summary>Reads a directory in the given address.</summary>
		/// <param name="address">Relative or fully qualified path.</param>
		/// <returns>Explorable directory.</returns>
		public static Directory ReadDirectory(string address)
		{
			// This checks whether the address is local or full:
			if (!address.IsFullAddress())
				address = Environment.CurrentDirectory + address;

			// Interpret all files and directories found within this directory:
			var directory = new Directory(null, Path.GetDirectoryName(address));
			
			// If doesn't exist, notify an error (and return an empty directory).
			if (!System.IO.Directory.Exists(address))
			{
				Log.SendError("Could not locate the directory.", address);
				return directory;
			}
			
			// Read the directory:
			ReadAll(directory);
			return directory;
		}

		/// <summary>Reads a file in the given address.</summary>
		/// <param name="address">Relative or fully qualified path.</param>
		/// <returns>File scope.</returns>
		public static File ReadFile(string address)
		{
			// This checks whether the address is local or full:
			if (!address.IsFullAddress())
				address = Environment.CurrentDirectory + address;
			var file = new File(null, Path.GetFileName(address));
			
			// If doesn't exist, notify an error (and return an empty file).
			if (!System.IO.File.Exists(address))
			{
				Log.SendError("Could not locate the file.", address);
				return file;
			}

			// Interpret the file:
			Interpret(file);
			return file;
		}

		/// <summary>
		/// Tokenizes a file.
		/// </summary>
		/// <param name="file">Clausewitz file.</param>
		/// <returns>Token list.</returns>
		internal static List<(string token, int line)> Tokenize(File file)
		{
			// The actual text data, character by character.
			var data = file.ReadText();

			// The current token so far recoreded since the last token-breaking character.
			var token = string.Empty;
			
			// All tokenized tokens within this file so far.
			var tokens = new List<(string token, int line)>();

			// Indicates a delimited string token.
			var @string = false;
			
			// Indicates a delimited comment token.
			var comment = false;

			// Counts each newline.
			var line = 0;
			
			// Keeps track of the previous char.
			var prevChar = '\0';
			
			// Tokenization loop:
			foreach (var @char in data)
			{
				// Keep tokenizing a string unless a switching delimiter comes.
				if (@string && (@char != '"'))
					goto tokenize;
				
				// Keep tokenizing a comment unless a switching delimiter comes.
				if (comment && !((@char == '\r') || (@char == '\n')))
					goto tokenize;
				
				// Standard tokenizer:
				var charToken = '\0';
				switch (@char)
				{
				// Newline: (also comment delimiter)
				case '\r':
				case '\n':
					comment = false;
					// Cross-platform compatibility for newlines:
					if ((prevChar == '\r') && (@char == '\n'))
						break;
					line++;
					break;
				
				// Whitespace (which breaks tokens):
				case ' ':
				case '\t':
					break;
				
				// String delimiter:
				case '"':
					@string = !@string;
					token += @char;
					break;
				
				// Comment delimiter:
				case '#':
					comment = true;
					charToken = @char;
					break;
				
				// Scope clauses & binding operator:
				case '}':
				case '{':
				case '=':
					charToken = @char;
					break;
				
				// Any other character:
				default:
					goto tokenize;
				}
				
				// Add new tokens to the list:
				if ((token.Length > 0) && !@string)
				{
					tokens.Add((token, line));
					token = "";
				}
				if (charToken != '\0')
					tokens.Add((new string(charToken, 1), line));
				prevChar = @char;
				continue;
				
				// Tokenize unfinished numbers/words/comments/strings.
				tokenize:
				token += @char;
				prevChar = @char;
			}

			// EOF & last token:
			if ((token.Length > 0) && !@string)
				tokens.Add((token, line));
			return tokens;
		}
		
		/// <summary>
		/// Interprets a file and all of its inner scopes recursively.
		/// </summary>
		/// <param name="file">A Clausewitz file.</param>
		internal static void Interpret(File file)
		{
			// Tokenize the file:
			var tokens = Tokenize(file);

			// All associated comments so far.
			var comments = new List<string>();

			// Current scope.
			Scope currentScope = file;
			
			// Interpretation loop:
			for (var index = 0; index < tokens.Count; index++)
			{
				// All current information:
				var token = tokens[index].token;
				var nextToken = index < (tokens.Count - 1) ?
					tokens[index + 1].token :
					string.Empty;
				var prevToken = index > 0 ?
					tokens[index - 1].token :
					string.Empty;
				var prevPrevToken = index > 1 ?
					tokens[index - 2].token :
					string.Empty;
				var details = string.Format("Token: '{0}'\nLine: {1}\nFile: {2}", token, tokens[index].line, file.Address);
				
				// Interpret tokens:
				switch (token)
				{
				// Enter a new scope:
				case "{":
				{
					// Participants:
					var name = prevPrevToken;
					var binding = prevToken;

					// Syntax check:
					if (binding == "=")
						if (IsValidValue(name))
							currentScope = currentScope.NewScope(name);
						else
							Log.SendError("Invalid name at scope binding.", details);
					else
						currentScope = currentScope.NewScope();

					AssociateComments(currentScope);
					
					break;

				}

				// Exit the current scope:
				case "}":
				{
					// Check if the current scope is the file, if so, then notify an error of a missing clause pair.
					if (!(currentScope is File))
						currentScope = currentScope.Parent;
					else
						Log.SendError("Missing scope clause pair for '}'.", details);
					
					// Associate end comments:
					if (comments.Count > 0)
					{
						currentScope.EndComments.AddRange(comments);
						comments.Clear();
					}
					break;
				}

				// Binding operator:
				case "=":
				{
					// Participants:
					var name = prevToken;
					var value = nextToken;

					// Skip scope binding: (handled at "{" case, otherwise will claim as syntax error.)
					if (value == "{")
						break;

					// Syntax check:
					if (!IsValidValue(name))
						Log.SendError("Invalid name at binding.", details);
					else if (!IsValidValue(value))
						Log.SendError("Invalid value at binding.", details);
					else
						currentScope.NewBinding(name, value);
					
					AssociateComments();
					break;
				}

				// Comment:
				case "#":
				{
					// Attached means the comment comes at the same line with another language construct:
					// If the comment comes at the same line with another construct, then it will be associated to that construct.
					// If the comment takes a whole line then it will be stacked and associated with the next construct when it is created.
					var isAttached = tokens[index].line == tokens[index - 1].line;
					if (isAttached)
						currentScope.Members.Last().Comments.Add(nextToken);
					else
						comments.Add(nextToken);
					break;
				}

				// Unattached value/word token:
				default:
				{
					// Check if bound:
					var isBound = prevToken.Contains("=") || nextToken.Contains("=");
					
					// Check if commented:
					var isComment = prevToken.Contains("#");
					
					// Skip those cases:
					if (!isBound && !isComment)
						if (IsValidValue(token))
							currentScope.NewToken(token);
						else
							Log.SendError("Unexpected token.", details);

					AssociateComments();
					break;
				}
				
				// End of switch body.
				}
			}

			// This local method helps with associating the stacking comments with the latest language construct.
			void AssociateComments(Construct construct = null, bool endComments = false)
			{
				if (comments.Count == 0)
					return;
				if (construct == null)
					construct = currentScope.Members.Last();
				if (!endComments)
				construct.Comments.AddRange(comments);
				comments.Clear();
			}
		}

		/// <summary>Reads & interprets all files or data found in the given address.</summary>
		/// <param name="parent">Parent directory.</param>
		private static void ReadAll(Directory parent)
		{
			// Read files:
			foreach (var file in System.IO.Directory.GetFiles(parent.Address))
				if (file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
					Interpret(parent.NewFile(Path.GetFileName(file)));
			// Read Directories:
			foreach (var directory in System.IO.Directory.GetDirectories(parent.Address))
				ReadAll(parent.NewDirectory(Path.GetDirectoryName(directory)));
		}
	}
}
