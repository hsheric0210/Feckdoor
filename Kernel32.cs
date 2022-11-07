using System.Runtime.InteropServices;
using System.Text;

namespace Feckdoor
{
	public static class Kernel32
	{
		[DllImport("kernel32.dll")]
		public static extern IntPtr GetConsoleWindow();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public unsafe static extern void *GlobalLock([In] IntPtr hMem);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return:MarshalAs(UnmanagedType.Bool)]
		public static extern bool GlobalUnlock([In] IntPtr hMem);
	}
}
