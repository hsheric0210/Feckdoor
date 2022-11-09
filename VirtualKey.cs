
// From fabriciorissetto/KeystrokeAPI
// https://github.com/fabriciorissetto/KeystrokeAPI/blob/master/Keystroke.API/Entities/KeyCode.cs

namespace Feckdoor
{
	internal static class VkHelper
	{
		private static readonly IDictionary<VirtualKey, string?> NameCache = new Dictionary<VirtualKey, string?>();
		private static readonly IDictionary<VirtualKey, string?> ShiftNameCache = new Dictionary<VirtualKey, string?>();
		private static readonly IDictionary<VirtualKey, ModifierKey> ModifierCache = new Dictionary<VirtualKey, ModifierKey>();

		private static V GetValueCached<K, V>(this VirtualKey vkCode, IDictionary<VirtualKey, V> cache, Func<K, V> getter, V? defaultValue = default)
		{
			if (cache.TryGetValue(vkCode, out V? value))
				return value;

			if (value == null)
				value = defaultValue;
			var attr = typeof(VirtualKey).GetMember(vkCode.ToString())?.FirstOrDefault()?.GetCustomAttributes(typeof(K), false)?.FirstOrDefault();
			if (attr != null)
				value = getter((K)attr);
			cache.Add(vkCode, value);
			return value;
		}


		private static string? GetName(this VirtualKey vkCode) => GetValueCached<VkNameAttribute, string?>(vkCode, NameCache, (attr) => attr.Name);

		private static string? GetShiftedName(this VirtualKey vkCode) => GetValueCached<VkShiftNameAttribute, string?>(vkCode, ShiftNameCache, (attr) => attr.Name);

		public static ModifierKey? GetModifier(this VirtualKey vkCode) => GetValueCached<VkModifierAttribute, ModifierKey>(vkCode, ModifierCache, (attr) => attr.ModifierKey);

		public static bool GetVkName(this VirtualKey vkCode, ModifierKey modifier, ref string prevName)
		{
			if (modifier.HasFlag(ModifierKey.Shift))
			{
				string? shifted = vkCode.GetShiftedName();
				if (shifted != null)
				{
					prevName = shifted;
					return true;
				}
			}

			string? name = vkCode.GetName();
			if (name != null)
			{
				prevName = name;
				return true;
			}

			// Ctrl hotkeys -> weird characters
			if (modifier.HasFlag(ModifierKey.Ctrl) && vkCode >= VirtualKey.A && vkCode <= VirtualKey.Z)
			{
				prevName = vkCode.ToString();
				return true;
			}

			return false;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	internal class VkNameAttribute : Attribute
	{
		public string Name
		{
			get;
		}

		public VkNameAttribute(string name)
		{
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	internal class VkModifierAttribute : Attribute
	{
		public ModifierKey ModifierKey
		{
			get;
		}

		public VkModifierAttribute(ModifierKey modifier)
		{
			ModifierKey = modifier;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	internal class VkShiftNameAttribute : Attribute
	{
		public string Name
		{
			get;
		}

		public VkShiftNameAttribute(string name)
		{
			Name = name;
		}
	}

	public enum VirtualKey
	{
		None = 0,
		[VkName("LMB")]
		LButton = 1,
		[VkName("RMB")]
		RButton = 2,
		Cancel = 3,
		[VkName("MMB")]
		MButton = 4,
		XButton1 = 5,
		XButton2 = 6,
		[VkName("Backspace")]
		Back = 8,
		Tab = 9,
		LineFeed = 10,
		Clear = 12,
		Enter = 13,
		[VkName("SHIFT")]
		[VkModifier(ModifierKey.Shift)]
		ShiftKey = 16,
		[VkName("CTRL")]
		[VkModifier(ModifierKey.Ctrl)]
		ControlKey = 17,
		[VkModifier(ModifierKey.Alt)]
		[VkName("ALT")]
		Menu = 18,
		Pause = 19,
		CapsLock = 20,
		[VkName("HAN-ENG")]
		HangulMode = 21,
		JunjaMode = 23,
		FinalMode = 24,
		[VkName("HANJA")]
		HanjaMode = 25,
		[VkName("ESC")]
		Escape = 27,
		IMEConvert = 28,
		IMENonconvert = 29,
		IMEAccept = 30,
		IMEModeChange = 31,
		Space = 32,
		PageUp = 33,
		PageDown = 34,
		End = 35,
		Home = 36,
		Left = 37,
		Up = 38,
		Right = 39,
		Down = 40,
		Select = 41,
		Print = 42,
		Execute = 43,
		PrintScreen = 44,
		Insert = 45,
		Delete = 46,
		Help = 47,
		[VkName("0")]
		[VkShiftName(")")]
		D0 = 48,
		[VkName("1")]
		[VkShiftName("!")]
		D1 = 49,
		[VkName("2")]
		[VkShiftName("@")]
		D2 = 50,
		[VkName("3")]
		[VkShiftName("#")]
		D3 = 51,
		[VkName("4")]
		[VkShiftName("$")]
		D4 = 52,
		[VkName("5")]
		[VkShiftName("%")]
		D5 = 53,
		[VkName("6")]
		[VkShiftName("^")]
		D6 = 54,
		[VkName("7")]
		[VkShiftName("&")]
		D7 = 55,
		[VkName("8")]
		[VkShiftName("*")]
		D8 = 56,
		[VkName("9")]
		[VkShiftName("(")]
		D9 = 57,
		A = 65,
		B = 66,
		C = 67,
		D = 68,
		E = 69,
		F = 70,
		G = 71,
		H = 72,
		I = 73,
		J = 74,
		K = 75,
		L = 76,
		M = 77,
		N = 78,
		O = 79,
		P = 80,
		Q = 81,
		R = 82,
		S = 83,
		T = 84,
		U = 85,
		V = 86,
		W = 87,
		X = 88,
		Y = 89,
		Z = 90,
		[VkName("WIN")]
		[VkModifier(ModifierKey.Win)]
		LWin = 91,
		[VkName("WIN")]
		[VkModifier(ModifierKey.Win)]
		RWin = 92,
		Apps = 93,
		Sleep = 95,
		NumPad0 = 96,
		NumPad1 = 97,
		NumPad2 = 98,
		NumPad3 = 99,
		NumPad4 = 100,
		NumPad5 = 101,
		NumPad6 = 102,
		NumPad7 = 103,
		NumPad8 = 104,
		NumPad9 = 105,
		[VkName("*")]
		Multiply = 106,
		[VkName("+")]
		Add = 107,
		Separator = 108,
		[VkName("-")]
		Subtract = 109,
		[VkName(".")]
		Decimal = 110,
		[VkName("/")]
		Divide = 111,
		F1 = 112,
		F2 = 113,
		F3 = 114,
		F4 = 115,
		F5 = 116,
		F6 = 117,
		F7 = 118,
		F8 = 119,
		F9 = 120,
		F10 = 121,
		F11 = 122,
		F12 = 123,
		F13 = 124,
		F14 = 125,
		F15 = 126,
		F16 = 127,
		F17 = 128,
		F18 = 129,
		F19 = 130,
		F20 = 131,
		F21 = 132,
		F22 = 133,
		F23 = 134,
		F24 = 135,
		NumLock = 144,
		[VkName("SCRLK")]
		Scroll = 145,
		[VkName("SHIFT")]
		[VkModifier(ModifierKey.Shift)]
		LShiftKey = 160,
		[VkName("SHIFT")]
		[VkModifier(ModifierKey.Shift)]
		RShiftKey = 161,
		[VkName("CTRL")]
		[VkModifier(ModifierKey.Ctrl)]
		LControlKey = 162,
		[VkName("CTRL")]
		[VkModifier(ModifierKey.Ctrl)]
		RControlKey = 163,
		[VkName("ALT")]
		[VkModifier(ModifierKey.Alt)]
		LMenu = 164,
		[VkName("ALT")]
		[VkModifier(ModifierKey.Alt)]
		RMenu = 165,
		BrowserBack = 166,
		BrowserForward = 167,
		BrowserRefresh = 168,
		BrowserStop = 169,
		BrowserSearch = 170,
		BrowserFavorites = 171,
		BrowserHome = 172,
		VolumeMute = 173,
		VolumeDown = 174,
		VolumeUp = 175,
		MediaNextTrack = 176,
		MediaPreviousTrack = 177,
		MediaStop = 178,
		MediaPlayPause = 179,
		LaunchMail = 180,
		SelectMedia = 181,
		LaunchApplication1 = 182,
		LaunchApplication2 = 183,
		[VkName(";")]
		[VkShiftName(":")]
		OemSemicolon = 186,
		[VkName("=")]
		[VkShiftName("+")]
		Oemplus = 187,
		[VkName(",")]
		[VkShiftName("<")]
		Oemcomma = 188,
		[VkName("-")]
		[VkShiftName("_")]
		OemMinus = 189,
		[VkName(".")]
		[VkShiftName(">")]
		OemPeriod = 190,
		[VkName("/")]
		[VkShiftName("?")]
		OemQuestion = 191,
		[VkName("`")]
		[VkShiftName("~")]
		Oemtilde = 192,
		LatinKeyboardBar = 193,
		[VkName(".")]
		[VkShiftName("Delete")]
		NumPadDot = 194,
		[VkName("[")]
		[VkShiftName("{")]
		OemOpenBrackets = 219,
		[VkName("\\")]
		[VkShiftName("|")]
		OemPipe = 220,
		[VkName("]")]
		[VkShiftName("}")]
		OemCloseBrackets = 221,
		[VkName("'")]
		[VkShiftName("\"")]
		OemQuotes = 222,
		Oem8 = 223,
		[VkName("\\")]
		[VkShiftName("|")]
		OemBackslash = 226,
		ProcessKey = 229,
		Packet = 231,
		Attn = 246,
		CrSel = 247,
		ExSel = 248,
		EraseEOF = 249,
		Play = 250,
		Zoom = 251,
		NoName = 252,
		Pa1 = 253,
		OemClear = 254
	}
}
