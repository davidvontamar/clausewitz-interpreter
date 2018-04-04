using System;
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
			Console.WriteLine("Welcome to Clausewitz interprter CLI.\nCopyright © 2018 - written by David von Tamar.");
			Console.ReadKey();
		}
	}
}
