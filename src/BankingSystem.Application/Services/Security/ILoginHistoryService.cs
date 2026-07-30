using BankingSystem.Application.DTOs.Security;

namespace BankingSystem.Application.Services.Security;

public interface ILoginHistoryService
{
    Task<List<LoginHistoryDto>> GetRecentAsync(int take = 200, CancellationToken ct = default);
}
