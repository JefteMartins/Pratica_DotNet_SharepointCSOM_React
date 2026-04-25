using Microsoft.AspNetCore.Mvc;
using SharePointCsomApi.Services;

namespace SharePointCsomApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ISharePointService _sharePointService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ISharePointService sharePointService, ILogger<TasksController> logger)
    {
        _sharePointService = sharePointService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        try
        {
            var tasks = await _sharePointService.GetTasksAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tarefas no SharePoint");
            return StatusCode(500, "Ocorreu um erro interno ao processar a lista de tarefas.");
        }
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed([FromQuery] int count = 100)
    {
        try
        {
            await _sharePointService.SeedDataAsync(count);
            return Ok($"Sucesso ao gerar {count} itens.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a execução do Seed no SharePoint");
            return StatusCode(500, "Erro ao injetar dados massivos. Verifique os logs do servidor.");
        }
    }
}
