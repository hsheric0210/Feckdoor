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

		private static int RollingIndex = 1;
		private static bool RollingRequested = false;

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

		private static void WriteUndone(string format)
		{
			string inputLogFile = string.Format(format, DateTime.Now, RollingIndex);

			// If already exists
			if (RollingRequested)
			{
				// Check if new file is already exists
				while (new FileInfo(inputLogFile).Exists)
				{
					inputLogFile = string.Format(format, DateTime.Now, ++RollingIndex);
				}
				RollingRequested = false;
			}

			Log.Debug("Writing input log to {file}.", inputLogFile);

			try
			{
				var queueCopy = new List<InputLogEntry>(UndoneQueue);
				UndoneQueue.Clear();

				// todo: sqlite db support
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

				long size = new FileInfo(inputLogFile).Length;
				if (size >= Config.TheConfig.InputLog.RollingSize)
				{
					RollingIndex++;
					RollingRequested = true;

					Log.Information("Input log rolling requested n={n} (size {sz} >= {limit}).", RollingIndex, size, Config.TheConfig.LogRollingSize);
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
