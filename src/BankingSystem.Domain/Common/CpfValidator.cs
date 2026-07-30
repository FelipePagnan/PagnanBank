namespace BankingSystem.Domain.Common;

/// <summary>
/// Validates a Brazilian CPF using the official check-digit algorithm.
/// Rejects malformed input and well-known invalid sequences (e.g. 111.111.111-11).
/// </summary>
public static class CpfValidator
{
    public static bool IsValid(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        if (digits.Length != 11)
            return false;

        // All identical digits (00000000000, 11111111111, ...) are invalid.
        if (digits.Distinct().Count() == 1)
            return false;

        var numbers = digits.Select(c => c - '0').ToArray();

        // First check digit.
        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += numbers[i] * (10 - i);
        var firstCheck = (sum * 10) % 11;
        if (firstCheck == 10) firstCheck = 0;
        if (firstCheck != numbers[9])
            return false;

        // Second check digit.
        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += numbers[i] * (11 - i);
        var secondCheck = (sum * 10) % 11;
        if (secondCheck == 10) secondCheck = 0;

        return secondCheck == numbers[10];
    }
}
