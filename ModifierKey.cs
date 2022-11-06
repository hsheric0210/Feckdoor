using System.Collections.Immutable;

namespace Feckdoor
{
	public class MkHelper
	{
		private static readonly IReadOnlyDictionary<ModifierKey, IReadOnlySet<VirtualKey>> CorrespondingVkCodes;

		static MkHelper()
		{
			// https://learn.microsoft.com/ko-kr/windows/win32/api/winuser/nf-winuser-getasynckeystate
			var builder = ImmutableDictionary.CreateBuilder<ModifierKey, IReadOnlySet<VirtualKey>>();
			builder.Add(ModifierKey.Ctrl, ImmutableHashSet.Create(VirtualKey.ControlKey));
			builder.Add(ModifierKey.Shift, ImmutableHashSet.Create(VirtualKey.ShiftKey));
			builder.Add(ModifierKey.Alt, ImmutableHashSet.Create(VirtualKey.Menu));
			builder.Add(ModifierKey.Win, ImmutableHashSet.Create(VirtualKey.LWin, VirtualKey.RWin));
			CorrespondingVkCodes = builder.ToImmutable();
		}

		public static ModifierKey GetActiveModifierKeys()
		{
			ModifierKey flag = 0;
			foreach (var entry in CorrespondingVkCodes)
				if (entry.Value.Any(vkCode => (User32.GetAsyncKeyState((int)vkCode) & 0x01) != 0))
					flag |= entry.Key;

			return flag;
		}
	}

	[Flags]
	public enum ModifierKey
	{
		None = 0,
		Ctrl = 1,
		Alt = 2,
		Shift = 4,
		Win = 8
	}
}
