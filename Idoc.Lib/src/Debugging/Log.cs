namespace Idoc.Lib
{

	/// <summary>
	/// Log message type
	/// </summary>
	public enum LogType
	{
		Log,
		Warning,
		Error
	}

	/// <summary>
	/// Represents a log message
	/// </summary>
	public class Log
	{
		/// <summary>
		/// Gets a value indicating if the log message is selected
		/// </summary>
		public bool isSelected;

		/// <summary>
		/// The 300 first chars of the message
		/// </summary>
		public string preview;
		/// <summary>
		/// The log message
		/// </summary>
		public string message;

		/// <summary>
		/// The type of the log
		/// </summary>
		public LogType type;

		/// <summary>
		/// Creates new Log
		/// </summary>
		/// <param name="isSelected">Is the log is selected</param>
		/// <param name="message">The message of the log</param>
		/// <param name="type">The type of the log</param>
		public Log(bool isSelected, string message, LogType type)
		{
			this.isSelected = isSelected;
			this.message = message;
			this.type = type;
			this.preview = message.Take(300);
		}

	}
}