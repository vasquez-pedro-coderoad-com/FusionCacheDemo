namespace FusionCacheDemo.Application.DTOs;

public record DriverDto(int Id, string? Name, int TruckId, bool IsActive);

public record CreateDriverDto(string Name, int TruckId, bool IsActive = true);

public record UpdateDriverDto(int Id, string Name, int TruckId, bool IsActive);
