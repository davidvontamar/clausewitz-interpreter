using System.Drawing;
using System.IO;
using System.Linq;
using Tamar.ANSITerm;
using Tamar.Clausewitz.Constructs;
using Directory = Tamar.Clausewitz.IO.Directory;
using FileScope = Tamar.Clausewitz.IO.FileScope;

namespace Tamar.Clausewitz.CLI
{
	/// <summary>
	/// The CLI: "Command Line Interface" of the Clausewitz interpreter helps the
	/// user/developer to interact with the
	/// interpreter through a console interface for some basic commands.
	/// </summary>
	public static class CLI
	{
		/// <summary>Main entry point.</summary>
		public static void Main()
		{
			Console.WriteLine("A .NET Interpreter for Clausewitz Engine.\nWritten by David von Tamar, LGPLv3");
			Log.MessageSent += LogMessage;
			Console.CursorVisible = true;
			var input = Interpreter.ReadFile(@"Test\input.txt");
			if (!errorOccured)
			{
				PrettyPrint(input.Parent.Parent);
				input.Name = "output.txt";
				input.Write();
			}
			Log.Send("Press any key to exit.");
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
		private static string ConcatTree(object root, object current, Alignment alignment = Alignment.Inner)
		{
			if (root == current)
				return string.Empty;
			var tree = string.Empty;
			var isLast = false;
			switch (current)
			{
				// Inside a scope/file:
				case Construct construct:
				{
					if (construct is FileScope file)
					{
						var parent = file.Parent;
						if (parent.Explorables.Last() == file)
							isLast = true;
						else if (parent.Explorables.First() == file)
						{
						}
						tree += ConcatTree(root, parent);
					}
					else
					{
						var parent = construct.Parent;
						if (parent.Members.Last() == construct)
							isLast = true;
						tree += ConcatTree(root, parent);
					}
					break;
				}

				// Inside a directory:
				case Directory directory:
				{
					var parent = directory.Parent;
					if (parent.Explorables.Last() == directory)
						isLast = true;
					tree += ConcatTree(root, parent);
					break;
				}

				// End of switch body.
			}
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

		/// <summary>Handles messages from the log.</summary>
		/// <param name="message">Message sent.</param>
		private static void LogMessage(Log.Message message)
		{
			switch (message.Type)
			{
				case Log.Message.Types.Error:
					errorOccured = true;
					Console.Write("ERROR", Color.White, Color.Red);
					break;
				case Log.Message.Types.Info:
					Console.Write("Info", Color.Cyan, Color.Blue);
					break;
				default:
					Console.Write("Message", Color.White, Color.DarkGray);
					break;
			}
			Console.WriteLine(' ' + message.Text, Color.White, Color.DarkBlue);
			if (string.IsNullOrWhiteSpace(message.Details))
				return;
			Console.WriteLine(message.Details, Color.White);
		}

		/// <summary>Pretty-prints a Clausewitz scope/file/directory.</summary>
		/// <param name="root">Initial scope (file, directory or just a random scope).</param>
		/// <param name="current">Used for recursive iteration through inner scopes.</param>
		private static void PrettyPrint(object root, object current = null)
		{
			if (current == null)
				current = root;
			if (current is Construct construct)
			{
				foreach (var comment in construct.Comments)
				{
					// Preceding comments:
					Console.Write(ConcatTree(root, construct, Alignment.Before), TreeFore, DefaultBack);
					Console.WriteLine(comment, CommentFore);
				}
			}
			Console.Write(ConcatTree(root, current, Alignment.Node), TreeFore, DefaultBack);
			switch (current)
			{
				case Binding binding:
					Console.Write(binding.Name, TokenFore, TokenBack);
					Console.Write(" = ", BindingFore, DefaultBack);
					Console.WriteLine(binding.Value, TokenFore, TokenBack);
					break;
				case Scope scope:
					if (scope is FileScope)
						Console.WriteLine(scope.Name, FileFore, FileBack);
					else if (!string.IsNullOrWhiteSpace(scope.Name))
						Console.WriteLine(scope.Name, ScopeFore, ScopeBack);
					else
					{
						Console.WriteLine(scope.Members.Count > 0 ?
							"┐" :
							"─", TreeFore, DefaultBack);
					}
					foreach (var member in scope.Members)
						PrettyPrint(root, member);
					foreach (var comment in scope.EndComments)
					{
						// End comments:
						Console.Write(ConcatTree(root, scope, Alignment.After), TreeFore, DefaultBack);
						Console.WriteLine("  " + comment, CommentFore);
					}
					break;
				case Directory directory:
					if (!string.IsNullOrWhiteSpace(directory.Name))
						Console.WriteLine(directory.Name + Path.DirectorySeparatorChar, DirectoryFore, DirectoryBack);
					foreach (var subDirectory in directory.Directories)
						PrettyPrint(root, subDirectory);
					foreach (var file in directory.Files)
						PrettyPrint(root, file);
					break;
				case Token token:
					Console.WriteLine(token.Value, TokenFore, TokenBack);
					break;
			}
			Console.ResetColor();
		}

		private static readonly Color BindingFore = Color.Magenta;
		private static readonly Color CommentFore = Color.FromArgb(0, 255, 0);
		private static readonly Color DefaultBack = Color.Black;
		private static readonly Color DirectoryBack = Color.FromArgb(128, 128, 0);
		private static readonly Color DirectoryFore = Color.Yellow;

		/// <summary>
		/// Indicates that an error occured during the interpretation time.
		/// </summary>
		private static bool errorOccured;

		private static readonly Color FileBack = Color.DarkCyan;
		private static readonly Color FileFore = Color.Cyan;
		private static readonly Color ScopeBack = Color.Blue;
		private static readonly Color ScopeFore = Color.Cyan;
		private static readonly Color TokenBack = Color.DarkBlue;
		private static readonly Color TokenFore = Color.White;
		private static readonly Color TreeFore = Color.DarkGray;

		/// <summary>
		/// Special enum for pretty-printing which determines the junctions at the tree
		/// hierarchy.
		/// </summary>
		private enum Alignment
		{
			Before,
			Node,
			After,
			Inner
		}
	}
}