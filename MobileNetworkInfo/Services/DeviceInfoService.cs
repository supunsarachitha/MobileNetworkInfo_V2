using System.Globalization;
using System.Runtime.InteropServices;
using Android.App;
using Android.Content;
using Android.Hardware.Display;
using Android.OS;
using Android.Views;
using MobileNetworkInfo.Helpers;
using Environment = Android.OS.Environment;

namespace MobileNetworkInfo.Services;

public readonly record struct UsageInfo(long Total, long Available)
{
	public long Used => Math.Max(Total - Available, 0);
	public double UsedFraction => Formatters.Fraction(Used, Total);
}

/// <summary>Reads hardware and operating system details.</summary>
public sealed class DeviceInfoService
{
	readonly Context context = Android.App.Application.Context;

	static int ApiLevel => (int)Build.VERSION.SdkInt;

	public string Model => Build.Model ?? DeviceInfo.Current.Model;

	public string Manufacturer => Capitalize(Build.Manufacturer ?? DeviceInfo.Current.Manufacturer);

	public string AndroidName => AndroidVersions.DisplayName(Build.VERSION.Release ?? "", ApiLevel);

	public string Headline => Model.StartsWith(Manufacturer, StringComparison.OrdinalIgnoreCase) ? Model : $"{Manufacturer} {Model}";

	public string Subtitle => $"{AndroidName} · API {ApiLevel}";

	public TimeSpan Uptime => TimeSpan.FromMilliseconds(SystemClock.ElapsedRealtime());

	public RowList GetDeviceRows() => new()
	{
		{ "Device name", DeviceInfo.Current.Name },
		{ "Model", Build.Model },
		{ "Manufacturer", Manufacturer },
		{ "Brand", Capitalize(Build.Brand) },
		{ "Device", Build.Device },
		{ "Product", Build.Product },
		{ "Board", Build.Board },
		{ "Hardware", Build.Hardware },
		{ "Form factor", DeviceInfo.Current.Idiom.ToString() },
		{ "Type", DeviceInfo.Current.DeviceType == DeviceType.Virtual ? "Emulator" : "Physical device" },
	};

	public RowList GetSystemRows()
	{
		var rows = new RowList
		{
			{ "Android version", AndroidName },
			{ "API level", ApiLevel.ToString(CultureInfo.InvariantCulture) },
			{ "Security patch", Formatters.SecurityPatch(Build.VERSION.SecurityPatch) },
			{ "Build number", Build.Display },
			{ "Build date", Formatters.EpochDate(Build.Time) },
			{ "Build type", Build.Type },
			{ "Kernel", Java.Lang.JavaSystem.GetProperty("os.version") },
			{ "Runtime", Java.Lang.JavaSystem.GetProperty("java.vm.version") is { } vm ? $"ART {vm}" : null },
			{ "Bootloader", CleanUnknown(Build.Bootloader) },
			{ "Baseband", CleanUnknown(Build.RadioVersion) },
		};

		if (OperatingSystem.IsAndroidVersionAtLeast(31) && Build.VERSION.MediaPerformanceClass > 0)
			rows.Add("Performance class", AndroidVersions.ReleaseName(Build.VERSION.MediaPerformanceClass) is { } pc ? $"Android {pc}" : null);

		rows.Add("Uptime", Formatters.Duration(Uptime));
		rows.Add("Fingerprint", Build.Fingerprint);
		return rows;
	}

	public RowList GetProcessorRows()
	{
		var frequencies = ReadCpuMaxFrequencies();
		var rows = new RowList();
		if (OperatingSystem.IsAndroidVersionAtLeast(31))
		{
			var soc = $"{CleanUnknown(Build.SocManufacturer)} {CleanUnknown(Build.SocModel)}".Trim();
			rows.Add("Chipset", soc);
		}

		rows.Add("Hardware", Build.Hardware);
		rows.Add("Cores", System.Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture));
		rows.Add("Architecture", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant());
		rows.Add("Clusters", AndroidConstants.CpuClusters(frequencies));
		if (frequencies.Count > 0)
			rows.Add("Max frequency", Formatters.FrequencyKhz(frequencies.Max()));
		rows.Add("Supported ABIs", string.Join(", ", Build.SupportedAbis ?? []));
		return rows;
	}

	static List<long> ReadCpuMaxFrequencies()
	{
		var result = new List<long>();
		for (var i = 0; i < System.Environment.ProcessorCount; i++)
		{
			try
			{
				var text = File.ReadAllText($"/sys/devices/system/cpu/cpu{i}/cpufreq/cpuinfo_max_freq");
				// Ignore implausible values (some emulators report a few kHz)
				if (long.TryParse(text.Trim(), out var khz) && khz >= 100_000)
					result.Add(khz);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// Not exposed on every device / emulator
			}
		}
		return result;
	}

	public UsageInfo GetMemory()
	{
		if (context.GetSystemService(Context.ActivityService) is not ActivityManager am)
			return default;
		var info = new ActivityManager.MemoryInfo();
		am.GetMemoryInfo(info);
		return new UsageInfo(info.TotalMem, info.AvailMem);
	}

	public UsageInfo GetStorage()
	{
		var path = Environment.DataDirectory?.AbsolutePath;
		if (path is null)
			return default;
		var stat = new StatFs(path);
		return new UsageInfo(stat.TotalBytes, stat.AvailableBytes);
	}

	public RowList GetMemoryRows()
	{
		var ram = GetMemory();
		var storage = GetStorage();
		return new RowList
		{
			{ "RAM total", Formatters.Bytes(ram.Total) },
			{ "RAM available", Formatters.Bytes(ram.Available) },
			{ "RAM in use", $"{Formatters.Bytes(ram.Used)} ({Formatters.Percent(ram.UsedFraction)})" },
			{ "Storage total", Formatters.Bytes(storage.Total) },
			{ "Storage free", Formatters.Bytes(storage.Available) },
			{ "Storage in use", $"{Formatters.Bytes(storage.Used)} ({Formatters.Percent(storage.UsedFraction)})" },
		};
	}

	public RowList GetDisplayRows()
	{
		var info = DeviceDisplay.Current.MainDisplayInfo;
		var metrics = context.Resources?.DisplayMetrics;
		var display = (context.GetSystemService(Context.DisplayService) as DisplayManager)?.GetDisplay(Display.DefaultDisplay);

		var rows = new RowList
		{
			{ "Resolution", $"{info.Width:0} × {info.Height:0} px" },
		};

		if (metrics is not null)
		{
			var dpi = (int)metrics.DensityDpi;
			rows.Add("Density", $"{dpi} dpi ({AndroidConstants.DensityBucket(dpi)} · {info.Density:0.##}×)");
			rows.Add("Size in dp", $"{info.Width / info.Density:0} × {info.Height / info.Density:0} dp");
			if (metrics.Xdpi > 0 && metrics.Ydpi > 0)
			{
				var inches = Math.Sqrt(Math.Pow(info.Width / metrics.Xdpi, 2) + Math.Pow(info.Height / metrics.Ydpi, 2));
				if (inches is > 2 and < 30)
					rows.Add("Screen size", $"≈ {inches:0.0}\"");
			}
		}

		rows.Add("Refresh rate", $"{info.RefreshRate:0.#} Hz");
		if (display is not null)
		{
			var rates = display.GetSupportedModes()?
				.Select(m => Math.Round(m.RefreshRate))
				.Distinct()
				.OrderBy(r => r)
				.ToList();
			if (rates is { Count: > 1 })
				rows.Add("Supported rates", string.Join(" · ", rates) + " Hz");

			if (GetHdrTypes(display) is { } hdrTypes)
			{
				var hdr = hdrTypes.Select(AndroidConstants.HdrType).OfType<string>().Distinct().ToList();
				rows.Add("HDR", hdr.Count > 0 ? string.Join(", ", hdr) : "Not supported");
			}

			if (OperatingSystem.IsAndroidVersionAtLeast(26))
				rows.Add("Wide color gamut", display.IsWideColorGamut);
		}

		rows.Add("Orientation", info.Orientation.ToString());
		rows.Add("Rotation", info.Rotation switch
		{
			DisplayRotation.Rotation90 => "90°",
			DisplayRotation.Rotation180 => "180°",
			DisplayRotation.Rotation270 => "270°",
			_ => "0°",
		});
		return rows;
	}

	static IEnumerable<int>? GetHdrTypes(Display display)
	{
		// HdrCapabilities.getSupportedHdrTypes is deprecated on API 34 in favour of the per-mode list
		if (OperatingSystem.IsAndroidVersionAtLeast(34))
			return display.GetMode()?.GetSupportedHdrTypes()?.Select(t => (int)t);
		if (OperatingSystem.IsAndroidVersionAtLeast(24))
		{
#pragma warning disable CA1422
			return display.GetHdrCapabilities()?.GetSupportedHdrTypes()?.Select(t => (int)t);
#pragma warning restore CA1422
		}
		return null;
	}

	static readonly (string Label, string Feature)[] Features =
	[
		("NFC", "android.hardware.nfc"),
		("Bluetooth LE", "android.hardware.bluetooth_le"),
		("GPS", "android.hardware.location.gps"),
		("Telephony", "android.hardware.telephony"),
		("eSIM", "android.hardware.telephony.euicc"),
		("Wi-Fi Direct", "android.hardware.wifi.direct"),
		("Wi-Fi Aware", "android.hardware.wifi.aware"),
		("Wi-Fi RTT", "android.hardware.wifi.rtt"),
		("Ultra-wideband", "android.hardware.uwb"),
		("Fingerprint", "android.hardware.fingerprint"),
		("Face unlock", "android.hardware.biometrics.face"),
		("IR blaster", "android.hardware.consumerir"),
		("USB host", "android.hardware.usb.host"),
		("Camera flash", "android.hardware.camera.flash"),
		("Front camera", "android.hardware.camera.front"),
		("Rear camera", "android.hardware.camera"),
	];

	public RowList GetFeatureRows()
	{
		var pm = context.PackageManager;
		var rows = new RowList();
		if (pm is null)
			return rows;

		foreach (var (label, feature) in Features)
			rows.Add(label, pm.HasSystemFeature(feature));
		return rows;
	}

	static string? CleanUnknown(string? value) =>
		string.IsNullOrWhiteSpace(value) || value.Equals("unknown", StringComparison.OrdinalIgnoreCase) ? null : value;

	static string Capitalize(string? value) =>
		string.IsNullOrEmpty(value) ? "" : char.ToUpperInvariant(value[0]) + value[1..];
}
