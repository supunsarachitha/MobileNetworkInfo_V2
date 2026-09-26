using CommunityToolkit.Mvvm.ComponentModel;

namespace MobileNetworkInfo.ViewModels;

public abstract class BaseViewModel : ObservableObject
{
	IDispatcherTimer? timer;

	/// <summary>Called when the page becomes visible (and when the app returns to the foreground).</summary>
	public virtual void OnAppearing()
	{
	}

	/// <summary>Called when the page is hidden or the app goes to the background.</summary>
	public virtual void OnDisappearing() => StopPolling();

	/// <summary>Runs <paramref name="tick"/> now and then on the UI thread every <paramref name="interval"/> until the page is hidden.</summary>
	protected void StartPolling(TimeSpan interval, Action tick)
	{
		StopPolling();
		tick();
		if (Application.Current?.Dispatcher is not { } dispatcher)
			return;
		timer = dispatcher.CreateTimer();
		timer.Interval = interval;
		timer.Tick += (_, _) => tick();
		timer.Start();
	}

	/// <summary>
	/// Async variant for ticks that read data off the UI thread. A tick never overlaps the previous one,
	/// and a failed read is skipped instead of crashing the app.
	/// </summary>
	protected void StartPolling(TimeSpan interval, Func<Task> tick)
	{
		var busy = false;
		StartPolling(interval, Run);

		async void Run()
		{
			if (busy)
				return;
			busy = true;
			try
			{
				await tick();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
			finally
			{
				busy = false;
			}
		}
	}

	protected void StopPolling()
	{
		timer?.Stop();
		timer = null;
	}
}
