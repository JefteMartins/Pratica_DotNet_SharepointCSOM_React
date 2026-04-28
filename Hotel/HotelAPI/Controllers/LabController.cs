using Microsoft.AspNetCore.Mvc;
using HotelAPI.Services;
using HotelAPI.Models;

namespace HotelAPI.Controllers;

[ApiController]
[Route("api/lab")]
public class LabController : ControllerBase
{
    private readonly ILabService _labService;
    private readonly ILogger<LabController> _logger;

    public LabController(ILabService labService, ILogger<LabController> logger)
    {
        _labService = labService;
        _logger = logger;
    }

    [HttpGet("debug-columns")]
    public async Task<IActionResult> GetDebugColumns()
    {
        try
        {
            var result = await _labService.GetFieldMappingsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks()
    {
        try
        {
            var tasks = await _labService.GetTasksAsync();
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
            await _labService.SeedDataAsync(count);
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
            var result = await _labService.GetTasksPagedAsync(pageSize, pos);
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
            var result = await _labService.GetTasksStreamAsync(pageSize);
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
            var result = await _labService.CreateItemsSequentialAsync(count);
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
            var result = await _labService.CreateItemsBatchedAsync(count, batchSize);
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
        _labService.SetStressMode(enabled);
        return Ok(new { StressMode = enabled });
    }

    [HttpPost("resilience/create")]
    public async Task<IActionResult> CreateResilient([FromQuery] string title = "Resilient Task")
    {
        try
        {
            var result = await _labService.CreateItemWithResilienceAsync(title);
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
            var result = await _labService.SearchTasksAsync(filters);
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
            var result = await _labService.DeleteItemsSequentialAsync(count);
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
            var result = await _labService.DeleteItemsBatchedAsync(count, batchSize);
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
            var result = await _labService.DeleteTasksByFilterAsync(filters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Deletion Lab (Filtered)");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("task")]
    public async Task<IActionResult> UpdateTask([FromBody] TaskUpdateModel task)
    {
        try
        {
            var success = await _labService.UpdateTaskAsync(task);
            return Ok(new { Success = success });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tarefa");
            return StatusCode(500, ex.Message);
        }
    }
}
