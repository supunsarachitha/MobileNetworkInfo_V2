using Android;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Telephony;
using Java.Net;
using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Services;

public enum ConnectionKind
{
	None,
	Wifi,
	Cellular,
	Ethernet,
	Other,
}

public sealed record NetworkSnapshot(
	ConnectionKind Kind,
	string Title,
	string? Detail,
	int? SignalDbm,
	SignalLevel Signal,
	bool IsVpn,
	RowList Connection,
	RowList? Wifi,
	RowList? Cellular,
	RowList? ServingCell,
	RowList Addresses);

public sealed record WifiNetwork(
	string Ssid,
	string Bssid,
	int Rssi,
	SignalLevel Level,
	int Frequency,
	int Channel,
	string Band,
	string Security,
	string? Width,
	string? Standard,
	bool IsConnected);

public sealed record WifiScanResult(IReadOnlyList<WifiNetwork> Networks, bool IsFresh);

/// <summary>Reads connection, Wi-Fi and cellular details from the Android framework.</summary>
public sealed class NetworkInfoService
{
	static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

	readonly Context context = Android.App.Application.Context;

	ConnectivityManager? Connectivity => context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;
	WifiManager? Wifi => context.GetSystemService(Context.WifiService) as WifiManager;
	TelephonyManager? Telephony => context.GetSystemService(Context.TelephonyService) as TelephonyManager;

	public bool HasFineLocation =>
		context.CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted;

	public bool IsLocationEnabled
	{
		get
		{
			if (context.GetSystemService(Context.LocationService) is not LocationManager lm)
				return false;
			if (OperatingSystem.IsAndroidVersionAtLeast(28))
				return lm.IsLocationEnabled;
			return lm.IsProviderEnabled(LocationManager.NetworkProvider) || lm.IsProviderEnabled(LocationManager.GpsProvider);
		}
	}

	public bool HasTelephony => context.PackageManager?.HasSystemFeature(PackageManager.FeatureTelephony) == true;

	public NetworkSnapshot GetSnapshot()
	{
		var cm = Connectivity;
		var network = cm?.ActiveNetwork;
		var caps = network is null ? null : cm!.GetNetworkCapabilities(network);
		var link = network is null ? null : cm!.GetLinkProperties(network);

		var isVpn = caps?.HasTransport(TransportType.Vpn) == true;
		var kind = caps switch
		{
			null => ConnectionKind.None,
			_ when caps.HasTransport(TransportType.Wifi) => ConnectionKind.Wifi,
			_ when caps.HasTransport(TransportType.Cellular) => ConnectionKind.Cellular,
			_ when caps.HasTransport(TransportType.Ethernet) => ConnectionKind.Ethernet,
			_ => ConnectionKind.Other,
		};

		var wifiInfo = kind == ConnectionKind.Wifi ? GetWifiInfo(caps) : null;
		var cellular = HasTelephony ? ReadCellular() : null;

		string title = kind switch
		{
			ConnectionKind.Wifi => "Wi-Fi",
			ConnectionKind.Cellular => "Mobile data",
			ConnectionKind.Ethernet => "Ethernet",
			ConnectionKind.Other when isVpn => "VPN",
			ConnectionKind.Other => "Connected",
			_ => "Offline",
		};

		string? detail = null;
		int? dbm = null;
		var level = SignalLevel.None;
		if (kind == ConnectionKind.Wifi && wifiInfo is not null)
		{
			detail = WifiHelper.CleanSsid(wifiInfo.SSID) ?? "Connected";
			dbm = wifiInfo.Rssi;
			level = SignalQuality.FromWifiRssi(wifiInfo.Rssi);
		}
		else if (kind == ConnectionKind.Cellular && cellular is not null)
		{
			detail = JoinNonEmpty(" · ", cellular.Carrier, cellular.Generation);
			dbm = cellular.Dbm;
			level = cellular.Level;
		}
		else if (kind == ConnectionKind.None)
		{
			detail = Wifi?.IsWifiEnabled == false && cellular?.Carrier is null ? "Wi-Fi is off" : "No active connection";
		}

		return new NetworkSnapshot(
			kind,
			title,
			detail,
			dbm,
			level,
			isVpn,
			BuildConnectionRows(kind, title, caps, link),
			wifiInfo is null ? null : BuildWifiRows(wifiInfo),
			cellular?.Rows,
			cellular?.ServingCell,
			BuildAddressRows(link));
	}

	RowList BuildConnectionRows(ConnectionKind kind, string title, NetworkCapabilities? caps, LinkProperties? link)
	{
		if (caps is null)
			return new RowList { { "Status", "Not connected" } };

		var validated = caps.HasCapability(NetCapability.Validated);
		var rows = new RowList
		{
			{ "Status", validated ? "Connected" : "Connected, no internet" },
			{ "Type", caps.HasTransport(TransportType.Vpn) && kind != ConnectionKind.Other ? $"{title} via VPN" : title },
			{ "Internet access", validated ? "Verified" : "Not verified" },
			{ "Metered", !caps.HasCapability(NetCapability.NotMetered) },
			{ "VPN", caps.HasTransport(TransportType.Vpn) ? "Active" : "Off" },
		};

		if (OperatingSystem.IsAndroidVersionAtLeast(28))
			rows.Add("Roaming", !caps.HasCapability(NetCapability.NotRoaming));

		if (caps.LinkDownstreamBandwidthKbps > 0)
			rows.Add("Download estimate", Formatters.BandwidthKbps(caps.LinkDownstreamBandwidthKbps));
		if (caps.LinkUpstreamBandwidthKbps > 0)
			rows.Add("Upload estimate", Formatters.BandwidthKbps(caps.LinkUpstreamBandwidthKbps));

		if (OperatingSystem.IsAndroidVersionAtLeast(24) && Connectivity is { } cm)
			rows.Add("Data Saver", AndroidConstants.DataSaver((int)cm.RestrictBackgroundStatus));

		if (OperatingSystem.IsAndroidVersionAtLeast(28) && link is not null)
		{
			var privateDns = link.IsPrivateDnsActive
				? link.PrivateDnsServerName ?? "Automatic"
				: "Off";
			rows.Add("Private DNS", privateDns);
		}

		return rows;
	}

	// ---------------------------------------------------------------- Wi-Fi

	WifiInfo? GetWifiInfo(NetworkCapabilities? caps)
	{
		// WifiManager.getConnectionInfo is deprecated on API 31+ but still returns the SSID/BSSID
		// when location access is granted; the transport info is used as a fallback.
#pragma warning disable CA1422
		var info = Wifi?.ConnectionInfo;
#pragma warning restore CA1422
		if (info is null && OperatingSystem.IsAndroidVersionAtLeast(29))
			info = caps?.TransportInfo as WifiInfo;
		return info;
	}

	RowList BuildWifiRows(WifiInfo info)
	{
		var ssid = WifiHelper.CleanSsid(info.SSID);
		var frequency = info.Frequency;
		var rows = new RowList
		{
			{ "Network name", ssid ?? (HasFineLocation ? "Hidden (turn on location)" : "Hidden (needs location access)") },
			{ "BSSID", WifiHelper.CleanBssid(info.BSSID) },
			{ "Signal", SignalQuality.Summary(info.Rssi, SignalQuality.FromWifiRssi(info.Rssi)) },
		};

		if (frequency > 0)
		{
			rows.Add("Frequency", $"{frequency} MHz ({WifiHelper.BandFromFrequency(frequency)})");
			var channel = WifiHelper.ChannelFromFrequency(frequency);
			if (channel > 0)
				rows.Add("Channel", channel.ToString());
		}

		if (OperatingSystem.IsAndroidVersionAtLeast(30))
			rows.Add("Standard", WifiHelper.StandardName((int)info.WifiStandard, frequency) is var std && std != "Unknown" ? std : null);

		if (OperatingSystem.IsAndroidVersionAtLeast(29) && info.TxLinkSpeedMbps > 0)
			rows.Add("Link speed", $"↑ {info.TxLinkSpeedMbps} Mbps  ·  ↓ {Math.Max(info.RxLinkSpeedMbps, 0)} Mbps");
		else if (info.LinkSpeed > 0)
			rows.Add("Link speed", $"{info.LinkSpeed} Mbps");

		if (OperatingSystem.IsAndroidVersionAtLeast(30) && info.MaxSupportedTxLinkSpeedMbps > 0)
			rows.Add("Max link speed", $"↑ {info.MaxSupportedTxLinkSpeedMbps} Mbps  ·  ↓ {info.MaxSupportedRxLinkSpeedMbps} Mbps");

		if (OperatingSystem.IsAndroidVersionAtLeast(31))
			rows.Add("Security", AndroidConstants.WifiSecurityType((int)info.CurrentSecurityType));

		rows.Add("Supported bands", SupportedBands());
		return rows;
	}

	string? SupportedBands()
	{
		if (Wifi is not { } wm)
			return null;
		var bands = new List<string> { "2.4 GHz" };
		if (wm.Is5GHzBandSupported())
			bands.Add("5 GHz");
		if (OperatingSystem.IsAndroidVersionAtLeast(30) && wm.Is6GHzBandSupported())
			bands.Add("6 GHz");
		if (OperatingSystem.IsAndroidVersionAtLeast(31) && wm.Is60GHzBandSupported())
			bands.Add("60 GHz");
		return string.Join(" · ", bands);
	}

	public bool IsWifiEnabled => Wifi?.IsWifiEnabled == true;

	/// <summary>
	/// Starts a Wi-Fi scan and waits for fresh results. Android throttles scans (4 per 2 minutes
	/// in the foreground); when throttled the most recent cached results are returned.
	/// </summary>
	public async Task<WifiScanResult> ScanWifiAsync(CancellationToken cancellationToken = default)
	{
		if (Wifi is not { } wm)
			return new WifiScanResult([], false);

		var startedAtMicros = SystemClock.ElapsedRealtime() * 1000;
		bool started;
		try
		{
#pragma warning disable CA1422 // startScan is deprecated but remains the only way to request a scan
			started = wm.StartScan();
#pragma warning restore CA1422
		}
		catch (Java.Lang.SecurityException)
		{
			started = false;
		}

		if (started)
		{
			for (var i = 0; i < 16; i++)
			{
				await Task.Delay(500, cancellationToken);
				if (wm.ScanResults?.Any(r => r.Timestamp >= startedAtMicros) == true)
					break;
			}
		}

		return new WifiScanResult(GetScanResults(), started);
	}

	IReadOnlyList<WifiNetwork> GetScanResults()
	{
		if (Wifi is not { } wm)
			return [];

		IList<ScanResult>? results;
		try
		{
			results = wm.ScanResults;
		}
		catch (Java.Lang.SecurityException)
		{
			return [];
		}

#pragma warning disable CA1422
		var connectedBssid = WifiHelper.CleanBssid(wm.ConnectionInfo?.BSSID);
#pragma warning restore CA1422

		return (results ?? [])
			.Select(r =>
			{
#pragma warning disable CA1422 // ScanResult.SSID is deprecated on API 33 in favour of WifiSsid
				var ssid = WifiHelper.CleanSsid(r.Ssid);
#pragma warning restore CA1422
				var bssid = WifiHelper.CleanBssid(r.Bssid) ?? "";
				return new WifiNetwork(
					ssid ?? "Hidden network",
					bssid,
					r.Level,
					SignalQuality.FromWifiRssi(r.Level),
					r.Frequency,
					WifiHelper.ChannelFromFrequency(r.Frequency),
					WifiHelper.BandFromFrequency(r.Frequency),
					WifiHelper.SecurityFromCapabilities(r.Capabilities),
					WifiHelper.ChannelWidthName((int)r.ChannelWidth) is var w && w != "Unknown" ? w : null,
					OperatingSystem.IsAndroidVersionAtLeast(30) ? ShortStandard((int)r.WifiStandard, r.Frequency) : null,
					connectedBssid is not null && string.Equals(bssid, connectedBssid, StringComparison.OrdinalIgnoreCase));
			})
			.OrderByDescending(n => n.IsConnected)
			.ThenByDescending(n => n.Rssi)
			.ToList();
	}

	/// <summary>"Wi-Fi 6" from "Wi-Fi 6 (802.11ax)"; null for unknown/legacy.</summary>
	static string? ShortStandard(int standard, int frequency)
	{
		var name = WifiHelper.StandardName(standard, frequency);
		return name.StartsWith("Wi-Fi", StringComparison.Ordinal) ? name[..name.IndexOf(" (", StringComparison.Ordinal)] : null;
	}

	// ---------------------------------------------------------------- Cellular

	sealed record CellularReading(string? Carrier, string? Generation, int? Dbm, SignalLevel Level, RowList Rows, RowList? ServingCell);

	CellularReading? ReadCellular()
	{
		if (Telephony is not { } tm)
			return null;

		var simState = (int)tm.SimState;
		var carrier = NullIfEmpty(tm.NetworkOperatorName);
		var strengths = GetCellSignalStrengths(tm);
		var cells = HasFineLocation ? GetAllCellInfo(tm) : null;
		var generation = NetworkGeneration(tm, strengths, cells);

		int? dbm = null;
		var level = SignalLevel.None;
		if (strengths is { Count: > 0 })
		{
			// Prefer the LTE anchor for NSA 5G (NR values are often unavailable while idle)
			var primary = strengths.FirstOrDefault(s => s is CellSignalStrengthLte && CellMath.IsValid(s.Dbm))
				?? strengths.FirstOrDefault(s => CellMath.IsValid(s.Dbm))
				?? strengths[0];
			if (CellMath.IsValid(primary.Dbm))
				dbm = primary.Dbm;
			level = SignalQuality.FromAndroidLevel(primary.Level);
		}
		else if (OperatingSystem.IsAndroidVersionAtLeast(28) && tm.SignalStrength is { } legacy)
		{
			level = SignalQuality.FromAndroidLevel(legacy.Level);
		}

		var rows = new RowList
		{
			{ "SIM", AndroidConstants.SimState(simState) },
			{ "Network operator", carrier },
			{ "SIM operator", NullIfEmpty(tm.SimOperatorName) },
		};

		if (OperatingSystem.IsAndroidVersionAtLeast(28))
		{
			var carrierId = NullIfEmpty(tm.SimCarrierIdName);
			if (!string.Equals(carrierId, tm.SimOperatorName, StringComparison.OrdinalIgnoreCase))
				rows.Add("Carrier", carrierId);
		}

		var mccMnc = NullIfEmpty(tm.NetworkOperator);
		if (mccMnc is { Length: >= 5 })
			rows.Add("MCC / MNC", $"{mccMnc[..3]} / {mccMnc[3..]}");
		rows.Add("Country", NullIfEmpty(tm.NetworkCountryIso)?.ToUpperInvariant());
		rows.Add("Network type", generation);

		if (dbm is { } d)
			rows.Add("Signal strength", SignalQuality.Summary(d, level));
		else if (level != SignalLevel.None)
			rows.Add("Signal strength", SignalQuality.Describe(level));

		if (strengths is not null)
			AddSignalDetails(rows, strengths);

		if (simState == 5 || carrier is not null)
		{
			rows.Add("Roaming", tm.IsNetworkRoaming);
			if (OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				try
				{
					rows.Add("Mobile data", tm.DataEnabled ? "On" : "Off");
				}
				catch (Java.Lang.SecurityException)
				{
				}
			}
		}

		rows.Add("Phone type", AndroidConstants.PhoneType((int)tm.PhoneType));
		var slots = OperatingSystem.IsAndroidVersionAtLeast(30) ? tm.ActiveModemCount : GetPhoneCount(tm);
		if (slots > 0)
			rows.Add("SIM slots", slots == 1 ? "Single SIM" : $"{slots} (multi-SIM)");

		return new CellularReading(carrier, generation, dbm, level, rows, cells is null ? null : BuildServingCellRows(cells));
	}

#pragma warning disable CA1422
	static int GetPhoneCount(TelephonyManager tm) => tm.PhoneCount;
#pragma warning restore CA1422

	static IList<CellSignalStrength>? GetCellSignalStrengths(TelephonyManager tm)
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(29))
			return null;
		try
		{
			return tm.SignalStrength?.CellSignalStrengths;
		}
		catch (Java.Lang.SecurityException)
		{
			return null;
		}
	}

	static IList<CellInfo>? GetAllCellInfo(TelephonyManager tm)
	{
		try
		{
			return tm.AllCellInfo;
		}
		catch (Java.Lang.SecurityException)
		{
			return null;
		}
	}

	static string? NetworkGeneration(TelephonyManager tm, IList<CellSignalStrength>? strengths, IList<CellInfo>? cells)
	{
		var hasNr = strengths?.Any(s => s is CellSignalStrengthNr) == true;
		var hasLte = strengths?.Any(s => s is CellSignalStrengthLte) == true;

		// The registered cell (needs location access) is the most reliable source
		var registered = cells?.FirstOrDefault(c => c.IsRegistered);
		if (registered is CellInfoLte)
			return hasNr ? "5G NSA (LTE + NR)" : "4G LTE";
		if (registered is not null)
			return TechnologyName(registered) is var tech && tech == "5G NR" ? "5G SA (NR)" : tech;

		if (strengths is { Count: > 0 })
		{
			if (hasNr && hasLte)
				return "5G NSA (LTE + NR)";
			if (hasNr)
				return "5G (NR)";
			if (hasLte)
				return "4G LTE";
			if (strengths.Any(s => s is CellSignalStrengthWcdma || s is CellSignalStrengthTdscdma))
				return "3G";
			if (strengths.Any(s => s is CellSignalStrengthGsm))
				return "2G GSM";
			if (strengths.Any(s => s is CellSignalStrengthCdma))
				return "CDMA";
		}

		// getNetworkType needs READ_PHONE_STATE from API 30; on older versions it is freely available.
		if (!OperatingSystem.IsAndroidVersionAtLeast(30))
		{
			try
			{
#pragma warning disable CA1422
				return AndroidConstants.NetworkType((int)tm.NetworkType);
#pragma warning restore CA1422
			}
			catch (Java.Lang.SecurityException)
			{
			}
		}

		return null;
	}

	static string TechnologyName(CellInfo cell) => cell switch
	{
		CellInfoLte => "4G LTE",
		CellInfoWcdma => "3G WCDMA",
		CellInfoGsm => "2G GSM",
		CellInfoCdma => "CDMA",
		_ when OperatingSystem.IsAndroidVersionAtLeast(29) && cell is CellInfoNr => "5G NR",
		_ when OperatingSystem.IsAndroidVersionAtLeast(29) && cell is CellInfoTdscdma => "3G TD-SCDMA",
		_ => "Unknown",
	};

	static void AddSignalDetails(RowList rows, IList<CellSignalStrength> strengths)
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(29))
			return;

		foreach (var s in strengths)
		{
			switch (s)
			{
				case CellSignalStrengthLte lte:
					rows.Add("LTE RSRP", CellMath.WithUnit(lte.Rsrp, "dBm"));
					rows.Add("LTE RSRQ", CellMath.WithUnit(lte.Rsrq, "dB"));
					rows.Add("LTE SINR", CellMath.WithUnit(lte.Rssnr, "dB"));
					rows.Add("LTE RSSI", CellMath.WithUnit(lte.Rssi, "dBm"));
					break;
				case CellSignalStrengthNr nr:
					rows.Add("5G SS-RSRP", CellMath.WithUnit(nr.SsRsrp, "dBm"));
					rows.Add("5G SS-RSRQ", CellMath.WithUnit(nr.SsRsrq, "dB"));
					rows.Add("5G SS-SINR", CellMath.WithUnit(nr.SsSinr, "dB"));
					break;
				case CellSignalStrengthWcdma wcdma:
					rows.Add("WCDMA RSCP", CellMath.WithUnit(wcdma.Dbm, "dBm"));
					if (OperatingSystem.IsAndroidVersionAtLeast(30))
						rows.Add("WCDMA Ec/No", CellMath.WithUnit(wcdma.EcNo, "dB"));
					break;
				case CellSignalStrengthGsm gsm:
					rows.Add("GSM RSSI", CellMath.WithUnit(gsm.Dbm, "dBm"));
					break;
			}
		}
	}

	static RowList BuildServingCellRows(IList<CellInfo> cells)
	{
		var registered = cells.Where(c => c.IsRegistered).ToList();
		var rows = new RowList();
		if (registered.Count == 0)
		{
			rows.Add("Status", "No serving cell reported");
			return rows;
		}

		var cell = registered[0];
		rows.Add("Technology", TechnologyName(cell));

		switch (cell)
		{
			case CellInfoLte lte when lte.CellIdentity is { } id:
				if (CellMath.IsValid(id.Ci))
				{
					var (enb, sector) = CellMath.SplitLteCellId(id.Ci);
					rows.Add("Cell ID (CI)", id.Ci.ToString());
					rows.Add("eNodeB / sector", $"{enb} / {sector}");
				}
				rows.Add("TAC", CellMath.Value(id.Tac));
				rows.Add("PCI", CellMath.Value(id.Pci));
				if (OperatingSystem.IsAndroidVersionAtLeast(24))
					rows.Add("EARFCN", CellMath.Value(id.Earfcn));
				if (OperatingSystem.IsAndroidVersionAtLeast(30))
					rows.Add("Band", FormatBands(id.GetBands()));
				if (OperatingSystem.IsAndroidVersionAtLeast(28) && CellMath.IsValid(id.Bandwidth) && id.Bandwidth > 0)
					rows.Add("Bandwidth", $"{id.Bandwidth / 1000d:0.#} MHz");
				AddMccMnc(rows, id);
				break;

			case CellInfoWcdma wcdma when wcdma.CellIdentity is { } id:
				rows.Add("Cell ID", CellMath.Value(id.Cid));
				rows.Add("LAC", CellMath.Value(id.Lac));
				rows.Add("PSC", CellMath.Value(id.Psc));
				if (OperatingSystem.IsAndroidVersionAtLeast(24))
					rows.Add("UARFCN", CellMath.Value(id.Uarfcn));
				AddMccMnc(rows, id);
				break;

			case CellInfoGsm gsm when gsm.CellIdentity is { } id:
				rows.Add("Cell ID", CellMath.Value(id.Cid));
				rows.Add("LAC", CellMath.Value(id.Lac));
				if (OperatingSystem.IsAndroidVersionAtLeast(24))
				{
					rows.Add("ARFCN", CellMath.Value(id.Arfcn));
					rows.Add("BSIC", CellMath.Value(id.Bsic));
				}
				AddMccMnc(rows, id);
				break;

			default:
				if (OperatingSystem.IsAndroidVersionAtLeast(29) && cell is CellInfoNr nr && nr.CellIdentity is CellIdentityNr nrId)
				{
					rows.Add("Cell ID (NCI)", CellMath.Value(nrId.Nci));
					rows.Add("TAC", CellMath.Value(nrId.Tac));
					rows.Add("PCI", CellMath.Value(nrId.Pci));
					rows.Add("NR-ARFCN", CellMath.Value(nrId.Nrarfcn));
					if (OperatingSystem.IsAndroidVersionAtLeast(30))
						rows.Add("Band", FormatBands(nrId.GetBands(), "n"));
					rows.Add("MCC / MNC", nrId.MccString is { } mcc && nrId.MncString is { } mnc ? $"{mcc} / {mnc}" : null);
				}
				break;
		}

		if (registered.Count > 1)
			rows.Add("Aggregated cells", registered.Count.ToString());
		var neighbours = cells.Count - registered.Count;
		if (neighbours > 0)
			rows.Add("Neighbour cells", neighbours.ToString());
		return rows;
	}

	static void AddMccMnc(RowList rows, CellIdentity id)
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(28))
			return;
		var (mcc, mnc) = id switch
		{
			CellIdentityLte lte => (lte.MccString, lte.MncString),
			CellIdentityWcdma wcdma => (wcdma.MccString, wcdma.MncString),
			CellIdentityGsm gsm => (gsm.MccString, gsm.MncString),
			_ => (null, null),
		};
		if (mcc is not null && mnc is not null)
			rows.Add("MCC / MNC", $"{mcc} / {mnc}");
	}

	static string? FormatBands(int[]? bands, string prefix = "B") =>
		bands is { Length: > 0 } ? string.Join(", ", bands.Select(b => prefix + b)) : null;

	// ---------------------------------------------------------------- IP

	static RowList BuildAddressRows(LinkProperties? link)
	{
		var rows = new RowList();
		if (link is null)
			return rows;

		var addresses = link.LinkAddresses ?? [];
		var ipv4 = addresses.Where(a => a.Address is Inet4Address).Select(a => $"{a.Address!.HostAddress}/{a.PrefixLength}");
		var ipv6 = addresses.Where(a => a.Address is Inet6Address).Select(a => a.Address!.HostAddress ?? "");

		rows.Add("Interface", link.InterfaceName);
		rows.Add("IPv4 address", string.Join(System.Environment.NewLine, ipv4));
		rows.Add("IPv6 address", string.Join(System.Environment.NewLine, ipv6.Where(a => a.Length > 0)));

		var gateways = (link.Routes ?? [])
			.Where(r => r.IsDefaultRoute && r.Gateway is { } g && !g.IsAnyLocalAddress)
			.Select(r => r.Gateway!.HostAddress)
			.Distinct();
		rows.Add("Gateway", string.Join(System.Environment.NewLine, gateways));
		rows.Add("DNS servers", string.Join(System.Environment.NewLine, (link.DnsServers ?? []).Select(d => d.HostAddress)));
		rows.Add("Search domain", link.Domains);

		if (OperatingSystem.IsAndroidVersionAtLeast(29) && link.Mtu > 0)
			rows.Add("MTU", $"{link.Mtu} bytes");
		if (link.HttpProxy is { } proxy && !string.IsNullOrEmpty(proxy.Host))
			rows.Add("Proxy", $"{proxy.Host}:{proxy.Port}");
		return rows;
	}

	/// <summary>Looks up the public IP address (user initiated). Returns null when offline.</summary>
	public async Task<string?> GetPublicIpAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var ip = (await Http.GetStringAsync("https://api.ipify.org", cancellationToken)).Trim();
			return System.Net.IPAddress.TryParse(ip, out _) ? ip : null;
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
		{
			return null;
		}
	}

	static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

	static string? JoinNonEmpty(string separator, params string?[] parts)
	{
		var present = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
		return present.Count == 0 ? null : string.Join(separator, present);
	}
}
