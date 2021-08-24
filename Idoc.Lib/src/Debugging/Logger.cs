using System.Collections.Generic;
using System;

namespace Idoc.Lib
{

	/// <summary>
	/// Logger class that not depends to the current platform where the application runs.
	/// </summary>
	/// <remarks>
	/// You can use this class in any platform like Unity or simple C# application window
	/// </remarks>
	public static class Logger
	{

		private static List<Log> logs;
		public static Action<Log> onlogged;

		public static List<Log> Logs
		{
			get => logs ?? (logs = new List<Log>());
			set => logs = value;
		}

		#region Methods
		private static void Add(Log log)
		{
			try
			{
				Logs.Add(log);
				onlogged?.Invoke(log);
			}
			catch
			{
				;
			}
		}

		public static void Clear()
		{
			Logs.Clear();
		}

		public static void LogSeparator()
		{
			Add(new Log(false, "------------------------------------------------", LogType.Log));

		}

		public static void Log(string message)
		{
			Add(new Log(false, message, LogType.Log));
		}

		public static void Log(string message, params object[] args)
		{
			message = string.Format(message, args);
			Add(new Log(false, message, LogType.Log));
		}

		public static void LogError(string message)
		{
			Add(new Log(false, message, LogType.Error));
		}

		public static void LogError(string message, params object[] args)
		{
			message = string.Format(message, args);
			Add(new Log(false, message, LogType.Error));
		}

		public static void LogWarning(string message)
		{
			Add(new Log(false, message, LogType.Warning));
		}

		public static void LogWarning(string message, params object[] args)
		{
			message = string.Format(message, args);
			Add(new Log(false, message, LogType.Warning));
		}

		#endregion Methods

	}
}