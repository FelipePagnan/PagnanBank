using BankingSystem.Application.DTOs.Store;
using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Services.Store;

public interface IStoreService
{
    Task<List<ProductDto>> GetCatalogAsync(CancellationToken ct = default);
    Task<Result<OrderSummaryDto>> CheckoutAsync(CheckoutRequest request, CancellationToken ct = default);
}
