namespace Feckdoor
{
	internal struct ActiveWindowInfo
	{
		public string Name;
		public string Executable;

		public bool Equals(ActiveWindowInfo other) => Name == other.Name && Executable == other.Executable;

		public static bool operator ==(ActiveWindowInfo first, ActiveWindowInfo second) => first.Equals(second);

		public static bool operator !=(ActiveWindowInfo first, ActiveWindowInfo second) => !(first == second);
	}
}
