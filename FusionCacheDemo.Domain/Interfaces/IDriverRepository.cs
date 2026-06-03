using FusionCacheDemo.Domain.Entities;

namespace FusionCacheDemo.Domain.Interfaces;

public interface IDriverRepository : IRepository<Driver>
{
    Task<IEnumerable<Driver>> GetActiveDriversAsync();
}
