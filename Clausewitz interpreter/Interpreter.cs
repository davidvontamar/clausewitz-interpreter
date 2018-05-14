using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Clausewitz.Constructs;
using Clausewitz.IO;
using Directory = Clausewitz.IO.Directory;
using File = Clausewitz.IO.File;
namespace Clausewitz
{
	/// <summary>The Clausewitz interpreter.</summary>
	public static class Interpreter
	{
		/// <summary>Reads a directory in the given address.</summary>
		/// <param name="address">Relative or fully qualified path.</param>
		/// <returns>Explorable directory.</returns>
		public static Directory ReadDirectory(string address)
		{
			// This checks whether the address is local or full:
			if (!address.IsFullAddress())
				address = Environment.CurrentDirectory + '\\' + address;

			// Interpret all files and directories found within this directory:
			var directory = address.DefineParents().NewDirectory(Path.GetFileNameWithoutExtension(address));

			// If doesn't exist, notify an error.
			if (!System.IO.Directory.Exists(address))
			{
				Log.SendError("Could not locate the directory.", address);
				return null;
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
				address = Environment.CurrentDirectory + '\\' + address;
			
			// Read the file:
			var file = address.DefineParents().NewFile(Path.GetFileName(address));
			return TryInterpret(file);
		}

		/// <summary>Translates data back into Clausewitz syntax and writes down to the actual file.</summary>
		/// <param name="file">Extended.</param>
		public static void Write(this File file)
		{
			file.WriteText(Translate(file));
			Log.Send("File saved: \"" + Path.GetFileName(file.Address) + "\".");
		}

		/// <summary>Translates data back into Clausewitz syntax and writes down all files within this directory.</summary>
		/// <param name="directory">Extended</param>
		public static void Write(this Directory directory)
		{
			foreach (var file in directory.Files)
				file.Write();
			foreach (var subDirectory in directory.Directories)
				subDirectory.Write();
		}

		/// <summary>Checks if a token is a valid value in Clausewitz syntax standards for both names & values.</summary>
		/// <param name="token">Token.</param>
		/// <returns>Boolean.</returns>
		internal static bool IsValidValue(string token)
		{
			return Regex.IsMatch(token, @"\d") || Regex.IsMatch(token, ValueRegexRule);
		}

		/// <summary>Tokenizes a file.</summary>
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

			// Indicates a new line.
			var newline = false;
			
			// Counts each newline.
			var line = 1;

			// Keeps track of the previous char.
			var prevChar = '\0';

			// Tokenization loop:
			foreach (var @char in data)
			{
				// Count a new line.
				if (newline)
				{
					line++;
					newline = false;
				}
				
				// Keep tokenizing a string unless a switching delimiter comes outside escape.
				if (@string && !((@char == '"') && (prevChar != '\\')))
					goto concat;

				// Keep tokenizing a comment unless a switching delimiter comes.
				if (comment && !((@char == '\r') || (@char == '\n')))
					goto concat;

				// Standard tokenizer:
				var charToken = '\0';
				switch (@char)
				{
				// Newline: (also comment delimiter)
				case '\r':
				case '\n':
					// Switch comments:
					if (comment)
					{
						comment = false;
						
						// Add empty comments:
						if (token.Length == 0)
							tokens.Add((string.Empty, line));
					}
					// Cross-platform compatibility for newlines:
					if ((prevChar == '\r') && (@char == '\n'))
						break;
					newline = true;
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
					goto concat;
				}

				// Add new tokens to the list:
				if ((token.Length > 0) && !@string)
				{
					tokens.Add((token, line));
					token = string.Empty;
				}
				if (charToken != '\0')
					tokens.Add((new string(charToken, 1), line));
				prevChar = @char;
				continue;

				// Concat characters to unfinished numbers/words/comments/strings.
				concat:
				token += @char;
				prevChar = @char;
			}

			// EOF & last token:
			if ((token.Length > 0) && !@string)
				tokens.Add((token, line));
			return tokens;
		}

		/// <summary>Translates data back into Clausewitz syntax.</summary>
		/// <param name="root">Root scope (file scope)</param>
		/// <returns>Clausewitz script.</returns>
		internal static string Translate(Scope root)
		{
			var data = string.Empty;
			var newline = Environment.NewLine;
			var tabs = new string('\t', root.Level);
			
			// Files include their own comments at the beginning followed by an empty line.
			if (root is File)
				if(root.Comments.Count > 0)
				{
					foreach (var comment in root.Comments)
						data += tabs + "# " + comment + newline;
					data += newline;
				}
			
			// Translate scope members:
			foreach (var construct in root.Members)
			{
				foreach (var comment in construct.Comments)
					data += tabs + "# " + comment + newline;

				// Translate the actual type:
				switch (construct)
				{
				case Scope scope:
					if (string.IsNullOrWhiteSpace(scope.Name))
						data += tabs + '{';
					else
						data += tabs + scope.Name + " = {";
					if (scope.Members.Count > 0)
					{
						data += newline + Translate(scope);
						foreach (var comment in scope.EndComments)
							data += tabs + "\t" + "# " + comment + newline;
						data += tabs + '}' + newline;
					}
					else
					{
						data += '}' + newline;
					}
					break;
				case Binding binding:
					data += tabs + binding.Name + " = " + binding.Value + newline;
					break;
				case Token token:
					if (root.Indented)
					{
						data += tabs + token.Value + newline;
					}
					else
					{
						var preceding = " ";
						var following = string.Empty;

						// Preceding characters:
						if (root.Members.First() == token)
							preceding = tabs;
						else if (token.Comments.Count > 0)
							preceding = tabs;
						else if (root.Members.First() != token)
							if (!(root.Members[root.Members.IndexOf(token) - 1] is Token))
								preceding = tabs;

						// Following characters:
						if (root.Members.Last() != token)
						{
							var next = root.Members[root.Members.IndexOf(token) + 1];
							if (!(next is Token))
								following = newline;
							if (next.Comments.Count > 0)
								following = newline;
						}
						else if (root.Members.Last() == token)
						{
							following = newline;
						}
						data += preceding + token.Value + following;
					}
					break;
				}
			}

			// Append end comments at files:
			if (root is File)
				foreach (var comment in root.EndComments)
					data += newline + "# " + comment;
			return data;
		}

		/// <summary>Interprets a file and all of its inner scopes recursively.</summary>
		/// <param name="file">A Clausewitz file.</param>
		/// <exception cref="SyntaxException">A syntax error was encountered during interpretation.</exception>
		private static void Interpret(File file)
		{
			// Tokenize the file:
			var tokens = Tokenize(file);

			// All associated comments so far.
			var comments = new List<(string text,int line)>();

			// Current scope.
			Scope scope = file;

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
				var line = tokens[index].line;

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
							scope = scope.NewScope(name);
						else
							throw new SyntaxException("Invalid name at scope binding.", file, line, token);
					else
						scope = scope.NewScope();
					AssociateComments(scope);
					break;
				}

				// Exit the current scope:
				case "}":
				{
					// Associate end comments:
					AssociateComments(scope, true);
					
					// Check if the current scope is the file, if so, then notify an error of a missing clause pair.
					if (!(scope is File))
					{
						if (scope.Sorted)
							scope.Sort();
						scope = scope.Parent;
					}
					else
					{
						throw new SyntaxException("Missing an openning pair '{' for a clause scope", file, line, token);
					}
					
					break;
				}

				// Binding operator:
				case "=":
				{
					// Participants:
					var name = prevToken;
					var value = nextToken;

					// Skip scope binding: (handled at "{" case, otherwise will claim as a syntax error.)
					if (value == "{")
						break;

					// Syntax check:
					if (!IsValidValue(name))
						throw new SyntaxException("Invalid name at binding.", file, line, token);
					if (!IsValidValue(value))
						throw new SyntaxException("Invalid value at binding.", file, line, token);
					scope.NewBinding(name, value);
					AssociateComments();
					break;
				}

				// Comment/pragma:
				case "#":
				{
					// Attached means the comment comes at the same line with another language construct:
					// If the comment comes at the same line with another construct, then it will be associated to that construct.
					// If the comment takes a whole line then it will be stacked and associated with the next construct when it is created.
					// If there was an empty line after the comment at the beginning of the file, then it will be associated with the file itself.
					// Comments are responsbile for pragmas as well when utilizing square brackets.
					var lineOfPrevToken = index > 0 ?
						tokens[index - 1].line :
						-1;
					var isAttached = line == lineOfPrevToken;

					// Associate attached comments HERE:
					if (isAttached)
					{
						if (prevToken != "{")
							scope.Members.Last().Comments.Add(nextToken.Trim());
						else
							scope.Comments.Add(nextToken.Trim());
					}
					else
						comments.Add((nextToken.Trim(), line));
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
						{
							scope.NewToken(token);
							AssociateComments();
						}
						else
						{
							throw new SyntaxException("Unexpected token.", file, line, token);
						}
					break;
				}

				// End of switch body.
				}
			}

			// Missing closing clause pair for scopes above the file level:
			if (scope != file)
				throw new SyntaxException("Missing a closing pair '}' for a clause scope.", file, tokens.Last().line, tokens.Last().token);
			
			// Associate end-comments (of the file):
			AssociateComments(scope, true);
			
			// This local method helps with associating the stacking comments with the latest language construct.
			void AssociateComments(Construct construct = null, bool endComments = false)
			{
				// No comments, exit.
				if (comments.Count == 0)
					return;
				
				// Associate with last construct if parameter is null.
				if ((construct == null) && (scope.Members.Count == 0))
					return;
				if (construct == null)
					construct = scope.Members.Last();
				
				// Leading comments at the beginning of a file:
				if (!endComments && construct.Parent is File && (construct.Parent.Members.First() == construct))
				{
					var associatedWithFile = new List<string>();
					var associatedWithConstruct = new List<string>();
					var associateWithFile = false;
					
					// Reverse iteration:
					for (var index = comments.Count - 1; index >= 0 ; index--) 
					{
						if(!associateWithFile)
						{
							var prevCommentLine = index < (comments.Count - 1) ?
								comments[index + 1].line :
								-1;
							var commentLine = comments[index].line;
							if ((prevCommentLine > 1) && ((prevCommentLine - commentLine) != 1))
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
					
				else if (construct is Scope commentScope)
					foreach (var comment in comments)
						commentScope.EndComments.Add(comment.text);
				comments.Clear();
			}
		}

		/// <summary>
		///     Reads & interprets all files or data found in the given address. It will attempt to load & interpret each
		///     file, however if an error has occurred it will skip the specific files.
		/// </summary>
		/// <param name="parent">Parent directory.</param>
		private static void ReadAll(Directory parent)
		{
			// Read files:
			foreach (var file in System.IO.Directory.GetFiles(parent.Address))
				if (file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
				{
					var newFile = parent.NewFile(Path.GetFileName(file));
					var interpretedFile = TryInterpret(newFile);
					if (interpretedFile == null)
						parent.Files.Remove(newFile);
				}

			// Read Directories:
			foreach (var directory in System.IO.Directory.GetDirectories(parent.Address))
				ReadAll(parent.NewDirectory(Path.GetFileNameWithoutExtension(directory)));
		}

		/// <summary>
		///     This method will try to interpret a file and handle potential syntax exceptions. If something went wrong
		///     during the interpretation it will rather not load the file at all, the user will be notified through an error
		///     message in the log, and the application will continue to run routinely.
		/// </summary>
		/// <param name="file">Clausewitz file.</param>
		/// <returns>Interpreted file or null if an error occurred.</returns>
		private static File TryInterpret(File file)
		{
			if (!System.IO.File.Exists(file.Address))
			{
				Log.SendError("Could not locate the file.", file.Address);
				return null;
			}
			try
			{
				Interpret(file);
				Log.Send("Loaded file: \"" + Path.GetFileName(file.Address) + "\".");
				return file;
			}
			catch (SyntaxException syntaxException)
			{
				syntaxException.Send();
				Log.Send("File was not loaded: \"" + Path.GetFileName(file.Address) + "\".");
			}
			return null;
		}

		/// <summary>
		///     Regex rule for valid Clausewitz values. Includes: identifiers, numerical values, and ':' variable binding
		///     operator.
		/// </summary>
		internal const string ValueRegexRule = @"[a-zA-Z0-9_.:""]+";

		/// <summary>
		///     Thrown when syntax-related errors occur during interpretation time which result in a broken & meaningless
		///     interpretation.
		/// </summary>
		public class SyntaxException : Exception
		{
			/// <summary>Primary constructor.</summary>
			/// <param name="message">Message.</param>
			/// <param name="file">The file where the exception occurred.</param>
			/// <param name="line">The line at which the exception occurred.</param>
			/// <param name="token">The token responsible for the exception.</param>
			internal SyntaxException(string message, File file, int line, string token) : base(message)
			{
				File = file;
				Line = line;
				Token = token;
			}

			/// <summary>Retrieves all detailed information in a formatted string.</summary>
			public string Details
			{
				get
				{
					return string.Format(
						"Token: '{0}'\nLine: {1}\nFile: {2}",
						Token,
						Line,
						File.Address.Remove(0, Environment.CurrentDirectory.Length));
				}
			}

			/// <summary>The file where the exception occurred.</summary>
			public readonly File File;

			/// <summary>The line at which the exception occurred.</summary>
			public readonly int Line;

			/// <summary>The token responsible for the exception.</summary>
			public readonly string Token;
		}
	}
}
