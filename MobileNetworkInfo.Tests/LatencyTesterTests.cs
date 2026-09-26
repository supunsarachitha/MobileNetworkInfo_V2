using System.Net;
using System.Net.Sockets;
using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Tests;

public class LatencyTesterTests : CultureTest
{
	[Fact]
	public async Task MeasureAsync_CollectsSamplesFromReachableHost()
	{
		using var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		var port = ((IPEndPoint)listener.LocalEndpoint).Port;

		var result = await LatencyTester.MeasureAsync(new LatencyTarget("Local", "127.0.0.1", port), attempts: 3,
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(result.Succeeded);
		Assert.Equal(3, result.Samples.Count);
		Assert.Equal(0, result.Lost);
		Assert.All(result.Samples, s => Assert.InRange(s, 0, 1000));
	}

	[Fact]
	public async Task MeasureAsync_ReportsRefusedConnections()
	{
		// Grab a free port and close it again so nothing is listening
		int port;
		using (var probe = new TcpListener(IPAddress.Loopback, 0))
		{
			probe.Start();
			port = ((IPEndPoint)probe.LocalEndpoint).Port;
		}

		var result = await LatencyTester.MeasureAsync(new LatencyTarget("Closed", "127.0.0.1", port), attempts: 2,
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.False(result.Succeeded);
		Assert.Equal("Connection refused", result.Error);
		Assert.Equal("Connection refused", result.Summary);
	}

	[Fact]
	public async Task MeasureAsync_ReportsDnsFailures()
	{
		var result = await LatencyTester.MeasureAsync(new LatencyTarget("Bad", "host.invalid", 443), attempts: 1,
			cancellationToken: TestContext.Current.CancellationToken);
		Assert.False(result.Succeeded);
		Assert.Equal("DNS lookup failed", result.Error);
	}

	[Fact]
	public void Result_ComputesStatistics()
	{
		var result = new LatencyResult(new LatencyTarget("T", "h", 1), [20, 30, 25, 25], 5, "Timed out");

		Assert.Equal(25, result.Average, 6);
		Assert.Equal(20, result.Min);
		Assert.Equal(30, result.Max);
		Assert.Equal(1, result.Lost);
		Assert.Equal((10 + 5 + 0) / 3d, result.Jitter, 6);
		Assert.Equal("25 ms · Excellent · jitter 5.0 ms · 1/5 failed", result.Summary);
	}

	[Fact]
	public void Result_SingleSampleHasNoJitter()
	{
		var result = new LatencyResult(new LatencyTarget("T", "h", 1), [12], 1, null);
		Assert.Equal(0, result.Jitter);
		Assert.Equal("12 ms · Excellent · jitter 0.0 ms", result.Summary);
	}

	[Theory]
	[InlineData(20, "Excellent")]
	[InlineData(75, "Good")]
	[InlineData(150, "Fair")]
	[InlineData(450, "Slow")]
	public void Verdict_RatesAverageLatency(double ms, string expected) =>
		Assert.Equal(expected, new LatencyResult(new LatencyTarget("T", "h", 1), [ms], 1, null).Verdict);
}
