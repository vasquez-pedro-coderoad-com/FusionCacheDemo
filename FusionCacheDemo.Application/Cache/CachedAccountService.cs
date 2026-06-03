using FusionCacheDemo.Application.DTOs;
using FusionCacheDemo.Application.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace FusionCacheDemo.Application.Cache;

public class CachedAccountService(
    IAccountService innerService,
    IFusionCache cache) : IAccountService
{
    private const string CachePrefix = "account";
    private const string AllAccountsKey = "accounts:all";

    public async Task<IEnumerable<AccountDto>> GetAllAsync()
    {
        return await cache.GetOrSetAsync(
            AllAccountsKey,
            async ct => await innerService.GetAllAsync(),
            options => options.SetDuration(TimeSpan.FromMinutes(5))) ?? [];
    }

    public async Task<AccountDto?> GetByIdAsync(int id)
    {
        return await cache.GetOrSetAsync(
            $"{CachePrefix}:{id}",
            async ct => await innerService.GetByIdAsync(id),
            options => options.SetDuration(TimeSpan.FromMinutes(10)));
    }

    public async Task<int> CreateAsync(CreateAccountDto dto)
    {
        var id = await innerService.CreateAsync(dto);
        await InvalidateListCacheAsync();
        return id;
    }

    public async Task<bool> UpdateAsync(UpdateAccountDto dto)
    {
        var result = await innerService.UpdateAsync(dto);

        if (result)
        {
            await cache.RemoveAsync($"{CachePrefix}:{dto.Id}");
            await InvalidateListCacheAsync();
        }

        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await innerService.DeleteAsync(id);

        if (result)
        {
            await cache.RemoveAsync($"{CachePrefix}:{id}");
            await InvalidateListCacheAsync();
        }

        return result;
    }

    private async Task InvalidateListCacheAsync()
    {
        await cache.RemoveAsync(AllAccountsKey);
    }
}
