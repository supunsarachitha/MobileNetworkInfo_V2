using System.Globalization;

namespace MobileNetworkInfo.Tests;

/// <summary>Formatting depends on the current culture; run the tests with a fixed one.</summary>
public abstract class CultureTest
{
	protected CultureTest()
	{
		CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
	}
}
