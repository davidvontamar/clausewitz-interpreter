using System;
using System.Collections.Generic;

namespace Tamar.Clausewitz
{
	/// <summary>
	/// Logs messages during operation time for the Clausewitz interpreter. Note that
	/// this class does not write a log
	/// file nor notify the user (or the developer) for any messages. Use the event
	/// handler to keep track of the messages,
	/// or write down to file the entire messages list.
	/// </summary>
	public static class Log
	{
		/// <summary>Sends a new message to the log.</summary>
		/// <param name="text">Main text line.</param>
		/// <param name="details">More details.</param>
		public static void Send(string text, string details = "")
		{
			Send(new Message(Message.Types.Info, text, details));
		}

		/// <summary>
		/// Sends an exception to the log.
		/// </summary>
		/// <param name="exception">Extended.</param>
		public static void Send(this Exception exception)
		{
			var details = string.Empty;
			if (exception is Interpreter.SyntaxException syntaxException)
				details = syntaxException.Details;
			var message = new Message(Message.Types.Error, exception.Message, details, exception);
			Send(message);
		}

		/// <summary>Sends a new message to the log.</summary>
		/// <param name="message">Log message.</param>
		public static void Send(Message message)
		{
			Messages.Add(message);
			MessageSent?.Invoke(message);
		}

		/// <summary>Sends a new error message to the log.</summary>
		/// <param name="text">Main text line.</param>
		/// <param name="details">More details.</param>
		/// <param name="exception">Exception thrown.</param>
		public static void SendError(string text, string details = "", Exception exception = null)
		{
			Send(new Message(Message.Types.Error, text, details, exception));
		}

		/// <summary>Contains all messages.</summary>
		public static readonly List<Message> Messages = new List<Message>();

		/// <summary>Special delegate to deliver the message as an event argument.</summary>
		/// <param name="message">The message that was sent.</param>
		public delegate void MessageHandler(Message message);

		/// <summary>
		/// Fires when a new message is sent. Use this event at Console applications or
		/// elsewhere to track log messages at
		/// runtime.
		/// </summary>
		public static event MessageHandler MessageSent;

		/// <summary>Log.Message struct.</summary>
		public struct Message
		{
			/// <summary>Primary constructor.</summary>
			/// <param name="type">Message type.</param>
			/// <param name="text">Main text line.</param>
			/// <param name="details">More details.</param>
			/// <param name="exception">Exception thrown.</param>
			public Message(Types type, string text, string details = "", Exception exception = null)
			{
				Exception = exception;
				Details = details;
				Text = text;
				Type = type;
			}

			/// <summary>More details such as filename.</summary>
			public string Details;

			/// <summary>Bound exception for errors.</summary>
			public Exception Exception;

			/// <summary>Leading text.</summary>
			public string Text;

			/// <summary>Message type.</summary>
			public Types Type;

			/// <summary>Message types.</summary>
			public enum Types
			{
				Info,
				Error
			}
		}
	}
}