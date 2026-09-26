using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace MobileNetworkInfo.Helpers;

public sealed record LatencyTarget(string Name, string Host, int Port);

public sealed record LatencyResult(LatencyTarget Target, IReadOnlyList<double> Samples, int Attempts, string? Error)
{
	public bool Succeeded => Samples.Count > 0;
	public double Average => Samples.Count > 0 ? Samples.Average() : double.NaN;
	public double Min => Samples.Count > 0 ? Samples.Min() : double.NaN;
	public double Max => Samples.Count > 0 ? Samples.Max() : double.NaN;
	public int Lost => Attempts - Samples.Count;

	/// <summary>Mean absolute difference between consecutive samples (RFC 3550 style jitter).</summary>
	public double Jitter
	{
		get
		{
			if (Samples.Count < 2)
				return 0;
			double sum = 0;
			for (var i = 1; i < Samples.Count; i++)
				sum += Math.Abs(Samples[i] - Samples[i - 1]);
			return sum / (Samples.Count - 1);
		}
	}

	/// <summary>Plain-language rating of the average latency.</summary>
	public string Verdict => Average switch
	{
		double.NaN => "Unreachable",
		< 50 => "Excellent",
		< 100 => "Good",
		< 200 => "Fair",
		_ => "Slow",
	};

	/// <summary>"24 ms · Excellent · jitter 3.2 ms" or an error description.</summary>
	public string Summary
	{
		get
		{
			if (!Succeeded)
				return Error ?? "Unreachable";
			var text = $"{Formatters.Milliseconds(Average)} · {Verdict} · jitter {Formatters.Milliseconds(Jitter)}";
			return Lost > 0 ? $"{text} · {Lost}/{Attempts} failed" : text;
		}
	}
}

/// <summary>
/// Measures round-trip latency as the time needed to open a TCP connection.
/// Unlike ICMP ping this needs no special permissions and works on every Android device.
/// </summary>
public static class LatencyTester
{
	public static readonly IReadOnlyList<LatencyTarget> DefaultTargets =
	[
		new("Cloudflare (1.1.1.1)", "1.1.1.1", 443),
		new("Google DNS (8.8.8.8)", "8.8.8.8", 443),
		new("Google (google.com)", "www.google.com", 443),
	];

	public static async Task<LatencyResult> MeasureAsync(LatencyTarget target, int attempts = 5, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
	{
		var perAttemptTimeout = timeout ?? TimeSpan.FromSeconds(3);
		var samples = new List<double>(attempts);
		string? error = null;

		IPAddress address;
		try
		{
			address = IPAddress.TryParse(target.Host, out var ip)
				? ip
				: (await Dns.GetHostAddressesAsync(target.Host, cancellationToken)).First();
		}
		catch (Exception ex) when (ex is SocketException or InvalidOperationException)
		{
			return new LatencyResult(target, samples, attempts, "DNS lookup failed");
		}

		for (var i = 0; i < attempts; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			attemptCts.CancelAfter(perAttemptTimeout);
			using var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
			var stopwatch = Stopwatch.StartNew();
			try
			{
				await socket.ConnectAsync(new IPEndPoint(address, target.Port), attemptCts.Token);
				stopwatch.Stop();
				samples.Add(stopwatch.Elapsed.TotalMilliseconds);
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				error = "Timed out";
			}
			catch (SocketException ex)
			{
				error = ex.SocketErrorCode switch
				{
					SocketError.NetworkUnreachable or SocketError.HostUnreachable => "Network unreachable",
					SocketError.ConnectionRefused => "Connection refused",
					SocketError.TimedOut => "Timed out",
					_ => "Connection failed",
				};
			}
		}

		return new LatencyResult(target, samples, attempts, error);
	}
}
