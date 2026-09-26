namespace MobileNetworkInfo.Controls;

/// <summary>Circular progress ring.</summary>
public sealed class RingGauge : GraphicsView, IDrawable
{
	public static readonly BindableProperty ProgressProperty =
		BindableProperty.Create(nameof(Progress), typeof(double), typeof(RingGauge), 0d,
			propertyChanged: (b, _, _) => ((RingGauge)b).Invalidate());

	public static readonly BindableProperty RingColorProperty =
		BindableProperty.Create(nameof(RingColor), typeof(Color), typeof(RingGauge), Colors.Green,
			propertyChanged: (b, _, _) => ((RingGauge)b).Invalidate());

	public static readonly BindableProperty ThicknessProperty =
		BindableProperty.Create(nameof(Thickness), typeof(float), typeof(RingGauge), 14f,
			propertyChanged: (b, _, _) => ((RingGauge)b).Invalidate());

	public RingGauge()
	{
		Drawable = this;
		Loaded += (_, _) => { if (Application.Current is { } app) app.RequestedThemeChanged += OnThemeChanged; };
		Unloaded += (_, _) => { if (Application.Current is { } app) app.RequestedThemeChanged -= OnThemeChanged; };
	}

	void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();

	public double Progress
	{
		get => (double)GetValue(ProgressProperty);
		set => SetValue(ProgressProperty, value);
	}

	public Color RingColor
	{
		get => (Color)GetValue(RingColorProperty);
		set => SetValue(RingColorProperty, value);
	}

	public float Thickness
	{
		get => (float)GetValue(ThicknessProperty);
		set => SetValue(ThicknessProperty, value);
	}

	public void Draw(ICanvas canvas, RectF rect)
	{
		var size = Math.Min(rect.Width, rect.Height) - Thickness;
		if (size <= 0)
			return;
		var x = rect.Center.X - size / 2;
		var y = rect.Center.Y - size / 2;

		canvas.StrokeSize = Thickness;
		canvas.StrokeLineCap = LineCap.Round;
		canvas.StrokeColor = ThemeColors.Get("SurfaceVariant");
		canvas.DrawEllipse(x, y, size, size);

		var progress = Math.Clamp(Progress, 0, 1);
		if (progress <= 0)
			return;
		canvas.StrokeColor = RingColor;
		if (progress >= 0.999)
			canvas.DrawEllipse(x, y, size, size);
		else
			canvas.DrawArc(x, y, size, size, 90, (float)(90 - 360 * progress), true, false);
	}
}
