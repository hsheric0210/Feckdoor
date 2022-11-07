using Serilog;

namespace Feckdoor.InputLog
{
	public class TimestampAppender : IDisposable
	{
		protected readonly CancellationTokenSource cancel;

		private bool disposed;

		public TimestampAppender()
		{
			cancel = new CancellationTokenSource();
			Task.Run(async () => await TimestampAppenderProc(cancel.Token));
		}

		private async static Task TimestampAppenderProc(CancellationToken ct)
		{
			try
			{
				while (!ct.IsCancellationRequested)
				{
					InputLogger.UndoneQueue.Enqueue(new TimestampEntry(DateTime.Now, Config.TheConfig.InputLog.PlainText.TimestampFormat));
					await Task.Delay(Config.TheConfig.InputLog.PlainText.TimestampDelay, ct);
				}
			}
			catch
			{
				// Ignore
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				cancel.Cancel();
				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
