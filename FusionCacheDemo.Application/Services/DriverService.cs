using FusionCacheDemo.Application.DTOs;
using FusionCacheDemo.Application.Interfaces;
using FusionCacheDemo.Domain.Entities;
using FusionCacheDemo.Domain.Interfaces;

namespace FusionCacheDemo.Application.Services;

public class DriverService(IDriverRepository repository) : IDriverService
{
    public async Task<IEnumerable<DriverDto>> GetAllAsync()
    {
        var drivers = await repository.GetAllAsync();
        return drivers.Select(d => new DriverDto(d.Id, d.Name, d.TruckId, d.IsActive));
    }

    public async Task<IEnumerable<DriverDto>> GetActiveDriversAsync()
    {
        var drivers = await repository.GetActiveDriversAsync();
        return drivers.Select(d => new DriverDto(d.Id, d.Name, d.TruckId, d.IsActive));
    }

    public async Task<DriverDto?> GetByIdAsync(int id)
    {
        var driver = await repository.GetByIdAsync(id);
        return driver is null ? null : new DriverDto(driver.Id, driver.Name, driver.TruckId, driver.IsActive);
    }

    public async Task<int> CreateAsync(CreateDriverDto dto)
    {
        var driver = new Driver
        {
            Name = dto.Name,
            TruckId = dto.TruckId,
            IsActive = dto.IsActive
        };
        return await repository.CreateAsync(driver);
    }

    public async Task<bool> UpdateAsync(UpdateDriverDto dto)
    {
        var driver = new Driver
        {
            Id = dto.Id,
            Name = dto.Name,
            TruckId = dto.TruckId,
            IsActive = dto.IsActive
        };
        return await repository.UpdateAsync(driver);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await repository.DeleteAsync(id);
    }
}
