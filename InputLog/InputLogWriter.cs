using Serilog;
using System.Diagnostics;
using System.Text;

namespace Feckdoor.InputLog
{
	internal static class InputLogWriter
	{
		private static readonly Queue<InputLogEntry> UndoneQueue = new();
		private static readonly Stopwatch InputTimer = new();
		private static volatile bool InputWriterRunning = true;
		private static Task WriterTask = null!;

		public static void Initialize()
		{
			WriterTask = Task.Run(async () =>
			{
				while (InputWriterRunning)
				{
					if (InputTimer.ElapsedMilliseconds > Config.TheConfig.InputLog.SaveWait && UndoneQueue.Count > 0 || UndoneQueue.Count >= Config.TheConfig.InputLog.SaveMaxUndone)
						WriteUndone(Config.TheConfig.InputLog.InputLogFile);

					await Task.Delay(Config.TheConfig.InputLog.SaveDelay);
				}

				// Final write (before shutdown)
				WriteUndone(Config.TheConfig.InputLog.InputLogFile);
			});
		}

		private static void WriteUndone(string inputLogFile)
		{
			Log.Debug("Writing input log.");

			try
			{
				var queueCopy = new List<InputLogEntry>(UndoneQueue);
				UndoneQueue.Clear();

				// current sqlite db is not supported

				using var writer = new StreamWriter(inputLogFile, true, Encoding.UTF8);
				foreach (var entry in queueCopy)
				{
					try
					{
						writer.Write(entry.PlainTextMessage);
					}
					catch (Exception e)
					{
						Log.Warning(e, "Exception during writing a input log entry.");
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e, "Exception during writing the input log.");
			}
		}

		public static void Push(InputLogEntry entry) => UndoneQueue.Enqueue(entry);

		public static void NotifyInput() => InputTimer.Restart();

		public static void Shutdown()
		{
			InputWriterRunning = false;
			WriterTask.Wait(); // Wait until the last save finishes
		}
	}
}
