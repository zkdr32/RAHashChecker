using System;
using System.Collections.Generic;
using System.Text;

namespace RAHashChecker
{
	public static class Logger
	{
		/// <summary>
		/// The system log. Subscribe to it to know what was logged.
		/// </summary>
		public static class Log
		{
			public static event Action<string> Logged;

			public static void Write(string message)
			{
				string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";
				Logged?.Invoke(line);
			}
		}
	}
}