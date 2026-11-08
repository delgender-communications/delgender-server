namespace Core.Utilities
{
    public static class PhoneNumbers
    {
        public static bool TryNormalize(string? value, out string normalized)
        {
            normalized = string.Empty;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var digits = new string(value.Where(char.IsAsciiDigit).ToArray());

            if (digits.StartsWith("0027") && digits.Length == 13)
            {
                normalized = "0" + digits[4..];
            }
            else if (digits.StartsWith("27") && digits.Length == 11)
            {
                normalized = "0" + digits[2..];
            }
            else if (digits.Length == 9 && digits[0] != '0')
            {
                normalized = "0" + digits;
            }
            else if (digits.Length == 10 && digits[0] == '0')
            {
                normalized = digits;
            }
            else
            {
                return false;
            }

            return true;
        }

        public static string Normalize(string? value) =>
            TryNormalize(value, out var normalized) ? normalized : (value ?? string.Empty).Trim();

        public static string? NormalizeOrNull(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : Normalize(value);
    }
}
