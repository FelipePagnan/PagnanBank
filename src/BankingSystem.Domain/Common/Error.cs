namespace BankingSystem.Domain.Common;

/// <summary>
/// Represents a business/domain error with a machine-readable code and a user-facing message.
/// </summary>
public sealed class Error
{
    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static readonly Error None = new(string.Empty, string.Empty);

    public override string ToString() => Message;
}
