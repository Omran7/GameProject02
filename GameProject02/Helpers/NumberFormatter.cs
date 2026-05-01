namespace GameProject02.Helpers;

public static class NumberFormatter
{
    public static string FormatNumber(long number)
    {
        if (number >= 1_000_000_000)
            return (number / 1_000_000_000.0).ToString("0.#") + "B";
        if (number >= 1_000_000)
            return (number / 1_000_000.0).ToString("0.#") + "M";
        if (number >= 1_000)
            return (number / 1_000.0).ToString("0.#") + "K";
        return number.ToString("N0");
    }

    public static string FormatNumber(int number) => FormatNumber((long)number);
}