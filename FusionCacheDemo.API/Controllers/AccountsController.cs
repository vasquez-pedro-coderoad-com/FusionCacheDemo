using FusionCacheDemo.Application.DTOs;
using FusionCacheDemo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FusionCacheDemo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(IAccountService accountService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll()
    {
        var accounts = await accountService.GetAllAsync();
        return Ok(accounts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountDto>> GetById(int id)
    {
        var account = await accountService.GetByIdAsync(id);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateAccountDto dto)
    {
        var id = await accountService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateAccountDto dto)
    {
        if (id != dto.Id) return BadRequest();
        var result = await accountService.UpdateAsync(dto);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await accountService.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
