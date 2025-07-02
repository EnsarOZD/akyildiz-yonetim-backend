using AkyildizYonetim.Domain.Entities;
using AkyildizYonetim.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.API.Controllers;

[ApiController]
[Route("api/aidat-definitions")]
public class AidatDefinitionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public AidatDefinitionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAidatDefinitions()
    {
        var list = await _context.AidatDefinitions.Where(x => x.IsActive).ToListAsync();
        return Ok(list);
    }
} 