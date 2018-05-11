using System;
using System.Linq;
using Clausewitz.Constructs;
using Clausewitz.IO;
namespace Clausewitz.CLI
{
	/// <summary>
	///     The CLI: "Command Line Interface" of the Clausewitz interpreter helps the user/developer to interact with the
	///     interpreter through a console interface for some basic commands.
	/// </summary>
	public static class CLI
	{
		/// <summary>Main entry point.</summary>
		public static void Main()
		{
			Console.WriteLine(
				"Welcome to the Clausewitz interprter CLI.\nCopyright © 2018 under LGPL v3.0 license. Written by David von Tamar.");
			Log.MessageSent += LogMessage;
			Console.CursorVisible = true;
			var input = Interpreter.ReadFile(@"test\input.txt");
			PrettyPrint(input.Parent.Parent);
			
			input.Name = "output.txt";
			input.Write();
			
			Log.Send("Operation finished, press any key to exit.");
			Console.ReadKey();
		}

		/// <summary>Draws tree structure to the left.</summary>
		/// <param name="root">The initial node. (will stop looking for parents beyond this member)</param>
		/// <param name="current">Current construct.</param>
		/// <param name="alignment">
		///     Whether this line opens a new node from its parent scope, or rather drawn above or beneath a
		///     node.
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
				if (construct is File file)
				{
					var parent = file.Parent;
					if (parent.Explorables.Last() == file)
					{
						isLast = true;
					}
					else if (parent.Explorables.First() == file)
					{ }
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
			if (alignment == Alignment.Before)
			{
				tree += "│ ";
			}
			else if ((alignment == Alignment.After) || (alignment == Alignment.Inner))
			{
				if (isLast)
					tree += "  ";
				else
					tree += "│ ";
			}
			else if (alignment == Alignment.Node)
			{
				if (isLast)
					tree += "└─";
				else
					tree += "├─";
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
				Console.BackgroundColor = ConsoleColor.Red;
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("ERROR");
				break;
			case Log.Message.Types.Info:
				Console.BackgroundColor = ConsoleColor.Blue;
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write("Info");
				break;
			default:
				Console.BackgroundColor = ConsoleColor.DarkGray;
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("Message");
				break;
			}
			Console.ResetColor();
			Console.BackgroundColor = ConsoleColor.DarkBlue;
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(' ' + message.Text);
			Console.ResetColor();
			if (string.IsNullOrWhiteSpace(message.Details))
				return;
			Console.ResetColor();
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(message.Details);
			Console.ResetColor();
		}

		/// <summary>Pretty-prints a Clausewitz scope/file/directory.</summary>
		/// <param name="root">Initial scope (file, directory or just a random scope).</param>
		/// <param name="current">Used for recursive iteration through inner scopes.</param>
		private static void PrettyPrint(object root, object current = null)
		{
			if (current == null)
				current = root;
			if (current is Construct construct && !(current is File))
				foreach (var comment in construct.Comments)
				{
					// Preceding comments:
					Console.BackgroundColor = DefaultBack;
					Console.ForegroundColor = TreeFore;
					Console.Write(ConcatTree(root, construct, Alignment.Before));
					Console.ResetColor();
					Console.ForegroundColor = CommentFore;
					Console.WriteLine(comment);
					Console.ResetColor();
				}
			Console.BackgroundColor = DefaultBack;
			Console.ForegroundColor = TreeFore;
			Console.Write(ConcatTree(root, current, Alignment.Node));
			Console.ResetColor();
			switch (current)
			{
			case Binding binding:
				Console.ResetColor();
				Console.ForegroundColor = TokenFore;
				Console.BackgroundColor = TokenBack;
				Console.Write(binding.Name);
				Console.BackgroundColor = DefaultBack;
				Console.ForegroundColor = BindingFore;
				Console.Write(" = ");
				Console.ForegroundColor = TokenFore;
				Console.BackgroundColor = TokenBack;
				Console.WriteLine(binding.Value);
				break;
			case Scope scope:
				Console.ResetColor();
				if (scope is File)
				{
					Console.ForegroundColor = FileFore;
					Console.BackgroundColor = FileBack;
					Console.WriteLine(scope.Name);
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(scope.Name))
					{
						Console.ForegroundColor = ScopeFore;
						Console.BackgroundColor = ScopeBack;
						Console.WriteLine(scope.Name);
					}
					else
					{
						Console.BackgroundColor = DefaultBack;
						Console.ForegroundColor = TreeFore;
						Console.WriteLine(
							scope.Members.Count > 0 ?
								'┐' :
								'─');
					}
				}
				Console.ResetColor();
				foreach (var member in scope.Members)
					PrettyPrint(root, member);
				foreach (var comment in scope.EndComments)
				{
					// End comments:
					Console.BackgroundColor = DefaultBack;
					Console.ForegroundColor = TreeFore;
					Console.Write(ConcatTree(root, scope, Alignment.After));
					Console.ResetColor();
					Console.ForegroundColor = CommentFore;
					Console.WriteLine("  " + comment);
					Console.ResetColor();
				}
				break;
			case Directory directory:
				if (!string.IsNullOrWhiteSpace(directory.Name))
				{
					Console.ResetColor();
					Console.ForegroundColor = DirectoryFore;
					Console.BackgroundColor = DirectoryBack;
					Console.WriteLine(directory.Name + '\\');
				}
				Console.ResetColor();
				foreach (var subDirectory in directory.Directories)
					PrettyPrint(root, subDirectory);
				foreach (var file in directory.Files)
					PrettyPrint(root, file);
				break;
			case Token token:
				Console.ResetColor();
				Console.ForegroundColor = TokenFore;
				Console.BackgroundColor = TokenBack;
				Console.WriteLine(token.Value);
				break;
			}
			Console.ResetColor();
		}

		private const ConsoleColor BindingFore = ConsoleColor.Magenta;
		private const ConsoleColor CommentFore = ConsoleColor.Green;
		private const ConsoleColor DefaultBack = ConsoleColor.Black;
		private const ConsoleColor DirectoryBack = ConsoleColor.DarkYellow;
		private const ConsoleColor DirectoryFore = ConsoleColor.Yellow;
		private const ConsoleColor FileBack = ConsoleColor.DarkCyan;
		private const ConsoleColor FileFore = ConsoleColor.Cyan;
		private const ConsoleColor ScopeBack = ConsoleColor.Blue;
		private const ConsoleColor ScopeFore = ConsoleColor.Cyan;
		private const ConsoleColor TokenBack = ConsoleColor.DarkBlue;
		private const ConsoleColor TokenFore = ConsoleColor.White;
		private const ConsoleColor TreeFore = ConsoleColor.DarkGray;

		/// <summary>Special enum for pretty-printing which determines the junctions at the tree hierarchy.</summary>
		private enum Alignment
		{
			Before,
			Node,
			After,
			Inner
		}
	}
}
