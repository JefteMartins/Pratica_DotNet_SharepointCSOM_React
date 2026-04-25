using Microsoft.AspNetCore.Mvc;
using SharePointCsomApi.Services;

namespace SharePointCsomApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly SharePointService _sharePointService;

    public TasksController(SharePointService sharePointService)
    {
        _sharePointService = sharePointService;
    }

    [HttpGet]
    public IActionResult GetTasks()
    {
        var tasks = _sharePointService.GetTasks();

        return Ok(tasks);
    }
}