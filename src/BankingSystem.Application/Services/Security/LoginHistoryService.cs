using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.DTOs.Security;

namespace BankingSystem.Application.Services.Security;

public sealed class LoginHistoryService : ILoginHistoryService
{
    private readonly ILoginHistoryRepository _loginHistory;

    public LoginHistoryService(ILoginHistoryRepository loginHistory)
        => _loginHistory = loginHistory;

    public async Task<List<LoginHistoryDto>> GetRecentAsync(int take = 200, CancellationToken ct = default)
    {
        var entries = await _loginHistory.GetRecentAsync(take, ct);
        return entries.Select(e => new LoginHistoryDto
        {
            TimestampUtc = e.TimestampUtc,
            Email = e.Email,
            Success = e.Success,
            ResultLabel = e.Success ? "Sucesso" : "Falha",
            Machine = e.Machine
        }).ToList();
    }
}
