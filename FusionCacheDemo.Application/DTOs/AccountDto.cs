namespace FusionCacheDemo.Application.DTOs;

public record AccountDto(int Id, string? Name, decimal Balance);

public record CreateAccountDto(string Name, decimal Balance);

public record UpdateAccountDto(int Id, string Name, decimal Balance);
