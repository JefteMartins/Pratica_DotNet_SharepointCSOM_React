using Microsoft.AspNetCore.Mvc;
using HotelAPI.Services;

namespace HotelAPI.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ISharePointProvisioningService _provisioningService;
    private readonly ISharePointSeedService _seedService;

    public AdminController(ISharePointProvisioningService provisioningService, ISharePointSeedService seedService)
    {
        _provisioningService = provisioningService;
        _seedService = seedService;
    }

    [HttpGet("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            var siteTitle = await _provisioningService.TestConnectionAsync();
            return Ok(new { Message = "Conexão bem sucedida!", SiteTitle = siteTitle });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Falha na conexão", Detail = ex.Message });
        }
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        try
        {
            await _seedService.SeedAsync();
            return Ok(new { Message = "Dados de teste inseridos com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message, Detail = ex.StackTrace });
        }
    }

    [HttpPost("provision")]
    public async Task<IActionResult> Provision()
    {
        try
        {
            await _provisioningService.ProvisionAsync();
            return Ok(new { Message = "Provisionamento realizado com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message, Detail = ex.StackTrace });
        }
    }
}
