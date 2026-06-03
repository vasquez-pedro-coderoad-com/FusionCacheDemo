using FusionCacheDemo.Application.DTOs;
using FusionCacheDemo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FusionCacheDemo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController(IDriverService driverService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DriverDto>>> GetAll()
    {
        var drivers = await driverService.GetAllAsync();
        return Ok(drivers);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<DriverDto>>> GetActive()
    {
        var drivers = await driverService.GetActiveDriversAsync();
        return Ok(drivers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DriverDto>> GetById(int id)
    {
        var driver = await driverService.GetByIdAsync(id);
        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateDriverDto dto)
    {
        var id = await driverService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateDriverDto dto)
    {
        if (id != dto.Id) return BadRequest();
        var result = await driverService.UpdateAsync(dto);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await driverService.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
