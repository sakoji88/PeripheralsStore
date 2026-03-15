namespace PeripheralsStore.Helpers;

public static class InputSanitizer
{
    public static string? Normalize(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
