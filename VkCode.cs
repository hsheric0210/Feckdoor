
// From fabriciorissetto/KeystrokeAPI
// https://github.com/fabriciorissetto/KeystrokeAPI/blob/master/Keystroke.API/Entities/KeyCode.cs
using System.Collections.Immutable;

namespace Feckdoor
{
	internal static class VkCodeHelper
	{
		public static bool GetVkName(this VkCode vkCode, bool shift, ref string prevName)
		{
			var mem = typeof(VkCode).GetMember(vkCode.ToString())?.FirstOrDefault();

			bool excluded = mem?.GetCustomAttributes(typeof(VkNameExcludedAttribute), false).Length > 0;
			if (excluded)
				return false;

			var attr = mem?.GetCustomAttributes(typeof(VkNameAttribute), false)?.FirstOrDefault();
			var shiftAttr = mem?.GetCustomAttributes(typeof(VkShiftNameAttribute), false)?.FirstOrDefault();

			if (shift && shiftAttr != null)
			{
				prevName = ((VkShiftNameAttribute)shiftAttr).Name;
				return true;
			}

			if (attr != null)
			{
				prevName = ((VkNameAttribute)attr).Name;
				return true;
			}

			if (prevName.Length > 1 && prevName.Length <= vkCode.ToString().Length)
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

	[AttributeUsage(AttributeTargets.Field)]
	internal class VkNameExcludedAttribute : Attribute
	{
	}

	public static class ModifierVkCode
	{
		public static IReadOnlyCollection<VkCode> CtrlKey { get; } = ImmutableList.Create(VkCode.Control, VkCode.ControlKey, VkCode.LControlKey, VkCode.RControlKey);
		public static IReadOnlyCollection<VkCode> ShiftKey { get; } = ImmutableList.Create(VkCode.Shift, VkCode.ShiftKey, VkCode.LShiftKey, VkCode.RShiftKey);
		public static IReadOnlyCollection<VkCode> WinKey { get; } = ImmutableList.Create(VkCode.LWin, VkCode.RWin);

		public static bool AnyVkMatch(this IReadOnlyCollection<VkCode> collection) => collection.Any(code => (User32.GetAsyncKeyState((int)code) & 1) != 0);
	}

	public enum VkCode
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
		ShiftKey = 16,
		[VkName("CTRL")]
		ControlKey = 17,
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
		LWin = 91,
		[VkName("WIN")]
		RWin = 92,
		Apps = 93,
		Sleep = 95,
		[VkNameExcluded]
		NumPad0 = 96,
		[VkNameExcluded]
		NumPad1 = 97,
		[VkNameExcluded]
		NumPad2 = 98,
		[VkNameExcluded]
		NumPad3 = 99,
		[VkNameExcluded]
		NumPad4 = 100,
		[VkNameExcluded]
		NumPad5 = 101,
		[VkNameExcluded]
		NumPad6 = 102,
		[VkNameExcluded]
		NumPad7 = 103,
		[VkNameExcluded]
		NumPad8 = 104,
		[VkNameExcluded]
		NumPad9 = 105,
		[VkNameExcluded]
		Multiply = 106,
		[VkNameExcluded]
		Add = 107,
		[VkNameExcluded]
		Separator = 108,
		[VkNameExcluded]
		Subtract = 109,
		[VkNameExcluded]
		Decimal = 110,
		[VkNameExcluded]
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
		LShiftKey = 160,
		[VkName("SHIFT")]
		RShiftKey = 161,
		[VkName("CTRL")]
		LControlKey = 162,
		[VkName("CTRL")]
		RControlKey = 163,
		[VkName("ALT")]
		LMenu = 164,
		[VkName("ALT")]
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
		OemClear = 254,
		KeyCode = 65535,
		[VkName("SHIFT")]
		Shift = 65536,
		[VkName("CTRL")]
		Control = 131072,
		[VkName("ALT")]
		Alt = 262144,
		Modifiers = -65536
	}
}
