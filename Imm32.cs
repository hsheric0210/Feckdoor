using System.Runtime.InteropServices;

namespace Feckdoor
{
	public static class Imm32
	{
		[DllImport("imm32.dll")]
		public static extern IntPtr ImmGetContext([In] IntPtr hWnd);

		[DllImport("imm32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ImmReleaseContext([In] IntPtr hWnd, [In] IntPtr hIMC);

		[DllImport("imm32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ImmGetCompositionStringA([In] IntPtr hWnd, [In] IntPtr hIMC, []);
	}
}
