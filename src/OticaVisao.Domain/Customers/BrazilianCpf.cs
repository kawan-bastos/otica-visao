namespace OticaVisao.Domain.Customers;

public static class BrazilianCpf
{
    public static string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (!IsValidDigits(digits)) throw new ArgumentException("Informe um CPF válido.", nameof(value));
        return digits;
    }

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && IsValidDigits(new string(value.Where(char.IsDigit).ToArray()));

    private static bool IsValidDigits(string digits)
    {
        if (digits.Length != 11 || digits.All(character => character == digits[0])) return false;
        return CalculateDigit(digits, 9, 10) == digits[9] - '0'
            && CalculateDigit(digits, 10, 11) == digits[10] - '0';
    }

    private static int CalculateDigit(string digits, int length, int initialWeight)
    {
        var sum = 0;
        for (var index = 0; index < length; index++) sum += (digits[index] - '0') * (initialWeight - index);
        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
