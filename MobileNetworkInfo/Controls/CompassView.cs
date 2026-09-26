namespace MobileNetworkInfo.Controls;

/// <summary>A compass dial that rotates so north on the dial points to magnetic north.</summary>
public sealed class CompassView : GraphicsView, IDrawable
{
	public static readonly BindableProperty HeadingProperty =
		BindableProperty.Create(nameof(Heading), typeof(double), typeof(CompassView), 0d,
			propertyChanged: (b, _, _) => ((CompassView)b).Invalidate());

	public CompassView()
	{
		Drawable = this;
		Loaded += (_, _) => { if (Application.Current is { } app) app.RequestedThemeChanged += OnThemeChanged; };
		Unloaded += (_, _) => { if (Application.Current is { } app) app.RequestedThemeChanged -= OnThemeChanged; };
	}

	void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();

	public double Heading
	{
		get => (double)GetValue(HeadingProperty);
		set => SetValue(HeadingProperty, value);
	}

	public void Draw(ICanvas canvas, RectF rect)
	{
		var radius = Math.Min(rect.Width, rect.Height) / 2 - 10;
		if (radius <= 0)
			return;
		var center = rect.Center;
		var text = ThemeColors.Get("TextPrimary");
		var secondary = ThemeColors.Get("TextSecondary");
		var accent = ThemeColors.Get("Primary");
		var north = ThemeColors.Get("PoorColor");

		canvas.FillColor = ThemeColors.Get("SurfaceVariant");
		canvas.FillCircle(center, radius);

		canvas.SaveState();
		canvas.Translate(center.X, center.Y);
		canvas.Rotate((float)-Heading);

		// Ticks every 5°, longer every 30°
		for (var deg = 0; deg < 360; deg += 5)
		{
			var major = deg % 30 == 0;
			var length = major ? 12f : deg % 10 == 0 ? 7f : 4f;
			var (sin, cos) = Math.SinCos(deg * Math.PI / 180);
			canvas.StrokeColor = deg == 0 ? north : major ? text : secondary;
			canvas.StrokeSize = major ? 2.5f : 1.2f;
			canvas.StrokeLineCap = LineCap.Round;
			canvas.DrawLine((float)(sin * (radius - 4)), (float)(-cos * (radius - 4)),
				(float)(sin * (radius - 4 - length)), (float)(-cos * (radius - 4 - length)));
		}

		// Cardinal letters and degree labels
		string[] cardinals = ["N", "E", "S", "W"];
		for (var i = 0; i < 12; i++)
		{
			var deg = i * 30;
			var (sin, cos) = Math.SinCos(deg * Math.PI / 180);
			var isCardinal = deg % 90 == 0;
			var r = radius - (isCardinal ? 34 : 30);
			var label = isCardinal ? cardinals[deg / 90] : deg.ToString();
			canvas.FontColor = deg == 0 ? north : isCardinal ? text : secondary;
			canvas.FontSize = isCardinal ? 18 : 11;
			canvas.Font = isCardinal ? Microsoft.Maui.Graphics.Font.DefaultBold : Microsoft.Maui.Graphics.Font.Default;
			canvas.DrawString(label, (float)(sin * r) - 20, (float)(-cos * r) - 10, 40, 20,
				HorizontalAlignment.Center, VerticalAlignment.Center);
		}

		// Needle: red half points to north on the dial
		var needle = radius * 0.42f;
		var northPath = new PathF();
		northPath.MoveTo(0, -needle);
		northPath.LineTo(7, 0);
		northPath.LineTo(-7, 0);
		northPath.Close();
		canvas.FillColor = north;
		canvas.FillPath(northPath);
		var southPath = new PathF();
		southPath.MoveTo(0, needle);
		southPath.LineTo(7, 0);
		southPath.LineTo(-7, 0);
		southPath.Close();
		canvas.FillColor = secondary;
		canvas.FillPath(southPath);
		canvas.RestoreState();

		canvas.FillColor = text;
		canvas.FillCircle(center, 4);

		// Fixed marker at the top: the direction the phone is pointing
		var marker = new PathF();
		marker.MoveTo(center.X, center.Y - radius + 2);
		marker.LineTo(center.X - 9, center.Y - radius - 9);
		marker.LineTo(center.X + 9, center.Y - radius - 9);
		marker.Close();
		canvas.FillColor = accent;
		canvas.FillPath(marker);
	}
}
