using FusionCacheDemo.Application.DTOs;
using FusionCacheDemo.Application.Interfaces;
using FusionCacheDemo.Domain.Entities;
using FusionCacheDemo.Domain.Interfaces;

namespace FusionCacheDemo.Application.Services;

public class AccountService(IAccountRepository repository) : IAccountService
{
    public async Task<IEnumerable<AccountDto>> GetAllAsync()
    {
        var accounts = await repository.GetAllAsync();
        return accounts.Select(a => new AccountDto(a.Id, a.Name, a.Balance));
    }

    public async Task<AccountDto?> GetByIdAsync(int id)
    {
        var account = await repository.GetByIdAsync(id);
        return account is null ? null : new AccountDto(account.Id, account.Name, account.Balance);
    }

    public async Task<int> CreateAsync(CreateAccountDto dto)
    {
        var account = new Account
        {
            Name = dto.Name,
            Balance = dto.Balance
        };
        return await repository.CreateAsync(account);
    }

    public async Task<bool> UpdateAsync(UpdateAccountDto dto)
    {
        var account = new Account
        {
            Id = dto.Id,
            Name = dto.Name,
            Balance = dto.Balance
        };
        return await repository.UpdateAsync(account);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await repository.DeleteAsync(id);
    }
}
