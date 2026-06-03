using FusionCacheDemo.Application.DTOs;

namespace FusionCacheDemo.Application.Interfaces;

public interface IDriverService
{
    Task<IEnumerable<DriverDto>> GetAllAsync();
    Task<IEnumerable<DriverDto>> GetActiveDriversAsync();
    Task<DriverDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateDriverDto dto);
    Task<bool> UpdateAsync(UpdateDriverDto dto);
    Task<bool> DeleteAsync(int id);
}
