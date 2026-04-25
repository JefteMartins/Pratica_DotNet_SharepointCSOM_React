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

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int pageSize = 100, [FromQuery] string? pos = null)
    {
        try
        {
            var result = await _sharePointService.GetTasksPagedAsync(pageSize, pos);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na paginação clássica");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("stream")]
    public async Task<IActionResult> GetStream([FromQuery] int pageSize = 100)
    {
        try
        {
            var result = await _sharePointService.GetTasksStreamAsync(pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na API de Stream");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("write/sequential")]
    public async Task<IActionResult> CreateSequential([FromQuery] int count = 10)
    {
        try
        {
            var result = await _sharePointService.CreateItemsSequentialAsync(count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Writing Lab (Sequential)");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("write/batched")]
    public async Task<IActionResult> CreateBatched([FromQuery] int count = 10, [FromQuery] int batchSize = 50)
    {
        try
        {
            var result = await _sharePointService.CreateItemsBatchedAsync(count, batchSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Writing Lab (Batched)");
            return StatusCode(500, ex.Message);
        }
    }
}
