using System.Globalization;

public static class NumberFormatter
{
    private static readonly string[] Suffixes = { "", " K", " M", " B", " T", " Qa", " Qi", " Sx", " Sp", " Oc" };

    public static string Format(double value, NumberFormatType formatType)
    {
        switch (formatType)
        {
            case NumberFormatType.Full:
                return value.ToString("0", CultureInfo.InvariantCulture);

            case NumberFormatType.Separated:
                return value.ToString("N0", CultureInfo.InvariantCulture);

            case NumberFormatType.Abbreviated:
                return FormatAbbreviated(value);

            default:
                return value.ToString("0", CultureInfo.InvariantCulture);
        }
    }

    private static string FormatAbbreviated(double value)
    {
        if (value < 1000)
            return value.ToString("0", CultureInfo.InvariantCulture);

        int magnitude = 0;
        while (value >= 1000 && magnitude < Suffixes.Length - 1)
        {
            value /= 1000;
            magnitude++;
        }

        return value.ToString("0.##", CultureInfo.InvariantCulture) + Suffixes[magnitude];
    }
}