using System;
using Clausewitz;
namespace Clausewitz.CLI
{
	/// <summary>
	/// The CLI: "Command Line Interface" of the Clausewitz interpreter helps the user/developer to interact with the interpreter through a console interface for some basic commands.
	/// </summary>
	public static class CLI
	{
		/// <summary>
		/// Main entry point.
		/// </summary>
		public static void Main()
		{
			Console.WriteLine("Welcome to the Clausewitz interprter CLI.\nCopyright © 2018 under the LGPL v3.0 license - written by David von Tamar.");
			Log.MessageSent += LogMessage;

			var input = Interpreter.ReadDirectory(@"test");

			Log.Send("Operation finished, press any key to exit.");
			Console.ReadKey();
		}

		/// <summary>
		/// Handles messages from the log.
		/// </summary>
		/// <param name="message">Message sent.</param>
		private static void LogMessage( Log.Message message )
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
			Console.Write(' ' + message.Text);
			FillLine();
			Console.ResetColor();
			if (string.IsNullOrWhiteSpace(message.Details))
				return;
			Console.ResetColor();
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(message.Details);
			FillLine();
			Console.ResetColor();
		}

		/// <summary>
		/// Fills the line with empty spaces till the end of the buffer.
		/// </summary>
		private static void FillLine()
		{
			for (var index = Console.CursorLeft; index < (Console.BufferWidth -1); index++)
				Console.Write(' ');
			Console.WriteLine();
		}
	}
}
