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

    [HttpPost("resilience/stress-toggle")]
    public IActionResult ToggleStress([FromQuery] bool enabled)
    {
        _sharePointService.SetStressMode(enabled);
        return Ok(new { StressMode = enabled });
    }

    [HttpPost("resilience/create")]
    public async Task<IActionResult> CreateResilient([FromQuery] string title = "Resilient Task")
    {
        try
        {
            var result = await _sharePointService.CreateItemWithResilienceAsync(title);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Resilience Lab");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchFilters filters)
    {
        try
        {
            var result = await _sharePointService.SearchTasksAsync(filters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Custom Search Lab");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("write/sequential")]
    public async Task<IActionResult> DeleteSequential([FromQuery] int count = 10)
    {
        try
        {
            var result = await _sharePointService.DeleteItemsSequentialAsync(count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Deletion Lab (Sequential)");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("write/batched")]
    public async Task<IActionResult> DeleteBatched([FromQuery] int count = 10, [FromQuery] int batchSize = 50)
    {
        try
        {
            var result = await _sharePointService.DeleteItemsBatchedAsync(count, batchSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Deletion Lab (Batched)");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("delete-by-filter")]
    public async Task<IActionResult> DeleteByFilter([FromBody] SearchFilters filters)
    {
        try
        {
            var result = await _sharePointService.DeleteTasksByFilterAsync(filters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Deletion Lab (Filtered)");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTask([FromBody] TaskUpdateModel task)
    {
        try
        {
            var success = await _sharePointService.UpdateTaskAsync(task);
            return Ok(new { Success = success });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tarefa");
            return StatusCode(500, ex.Message);
        }
    }
}
