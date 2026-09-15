using System;
using System.Globalization;

namespace EzCatalog.UnitTests.Common;

public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo originalCulture;

    public CultureScope(CultureInfo culture)
    {
        originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = culture;
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = originalCulture;
    }
}
