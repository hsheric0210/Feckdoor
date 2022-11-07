using Serilog;
using System.Diagnostics;
using System.Globalization;

namespace Feckdoor.InputLog
{
	public class KillswitchHandler : IDisposable
	{
		private readonly ISet<VirtualKey> KillswitchBinding;
		private readonly Stopwatch KillswitchTimer = new Stopwatch();
		private readonly long Deadline;

		private ISet<VirtualKey>? RemainingBind;
		private bool disposed;

		public KillswitchHandler()
		{
			Deadline = Config.TheConfig.KillswitchTimer;
			KillswitchBinding = Config.TheConfig.Killswitch.Split(' ').Select(vkCodeHex =>
			{
				// Remove hex identifier
				if (vkCodeHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
					vkCodeHex = vkCodeHex[2..];

				try
				{
					var vk = (VirtualKey)int.Parse(vkCodeHex, NumberStyles.HexNumber);
					Log.Debug("Parsed Killswitch key {key}.", vk);
					return vk;
				}
				catch (Exception e)
				{
					Log.Warning(e, "Exception during parsing Killswitch key code {key}.", vkCodeHex);
					return VirtualKey.None;
				}
			}).ToHashSet(); // Disable lazy initialization

			if (KillswitchBinding.Any(key => key != VirtualKey.None))
			{
				Log.Debug("Killswitch handler established.");
				KeyboardHook.OnKeyboardInput += CheckKillswitch;
			}
		}

		internal void CheckKillswitch(object? sender, KeyboardInputEventArgs args)
		{
			foreach (var key in RemainingBind ?? KillswitchBinding)
			{
				if (args.VkCodeEnum == key)
				{
					if (RemainingBind == null)
					{
						// Timer start!
						RemainingBind = new HashSet<VirtualKey>(KillswitchBinding);
						KillswitchTimer.Restart();
					}
					RemainingBind.Remove(key);

					Log.Debug("Killswitch binding part {key} press. {leftkeys} left. Timer {timer}ms left.", key, string.Join(", ", RemainingBind), Deadline - KillswitchTimer.ElapsedMilliseconds);
				}
			}

			if (KillswitchTimer.ElapsedMilliseconds > Deadline)
			{
				// Timeover!
				RemainingBind = null;
			}

			if ((RemainingBind?.Count ?? -1) == 0 && KillswitchTimer.ElapsedMilliseconds <= Deadline)
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
