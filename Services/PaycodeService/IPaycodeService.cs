using CostAccounting.Models;

namespace CostAccounting.Services.PaycodeService
{
    public interface IPaycodeService
    {
        Task<PaycodeIndexVM> GetPagedAsync(PaycodeIndexVM q);
        Task<(bool ok, string message)> CreateAsync(PaycodeVM vm, string user);
        Task<(bool ok, string message)> UpdateAsync(PaycodeVM vm);

        //Task<(bool Success, string Message)> SoftDeleteAsync(int objectId);
    }
}
