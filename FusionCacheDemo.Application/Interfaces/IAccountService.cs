using FusionCacheDemo.Application.DTOs;

namespace FusionCacheDemo.Application.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllAsync();
    Task<AccountDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateAccountDto dto);
    Task<bool> UpdateAsync(UpdateAccountDto dto);
    Task<bool> DeleteAsync(int id);
}
