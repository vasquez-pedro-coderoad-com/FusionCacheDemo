using FusionCacheDemo.Application.DTOs;
using FusionCacheDemo.Application.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace FusionCacheDemo.Application.Cache;

public class CachedDriverService(
    IDriverService innerService,
    IFusionCache cache) : IDriverService
{
    private const string CachePrefix = "driver";
    private const string AllDriversKey = "drivers:all";
    private const string ActiveDriversKey = "drivers:active";

    public async Task<IEnumerable<DriverDto>> GetAllAsync()
    {
        return await cache.GetOrSetAsync(
            AllDriversKey,
            async ct => await innerService.GetAllAsync(),
            options => options.SetDuration(TimeSpan.FromMinutes(5))) ?? [];
    }

    public async Task<IEnumerable<DriverDto>> GetActiveDriversAsync()
    {
        return await cache.GetOrSetAsync(
            ActiveDriversKey,
            async ct => await innerService.GetActiveDriversAsync(),
            options => options.SetDuration(TimeSpan.FromMinutes(5))) ?? [];
    }

    public async Task<DriverDto?> GetByIdAsync(int id)
    {
        return await cache.GetOrSetAsync(
            $"{CachePrefix}:{id}",
            async ct => await innerService.GetByIdAsync(id),
            options => options.SetDuration(TimeSpan.FromMinutes(10)));
    }

    public async Task<int> CreateAsync(CreateDriverDto dto)
    {
        var id = await innerService.CreateAsync(dto);
        await InvalidateListCachesAsync();
        return id;
    }

    public async Task<bool> UpdateAsync(UpdateDriverDto dto)
    {
        var result = await innerService.UpdateAsync(dto);

        if (result)
        {
            await cache.RemoveAsync($"{CachePrefix}:{dto.Id}");
            await InvalidateListCachesAsync();
        }

        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await innerService.DeleteAsync(id);

        if (result)
        {
            await cache.RemoveAsync($"{CachePrefix}:{id}");
            await InvalidateListCachesAsync();
        }

        return result;
    }

    private async Task InvalidateListCachesAsync()
    {
        await cache.RemoveAsync(AllDriversKey);
        await cache.RemoveAsync(ActiveDriversKey);
    }
}
