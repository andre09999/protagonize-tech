using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Dtos;
using TaskManager.Api.Entities;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private static readonly string[] AllowedStatus = ["Pendente", "Concluida"];

    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll([FromQuery] string? status)
    {
        var query = _context.Tasks.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(task => task.Status == status);
        }

        var tasks = await query
            .OrderByDescending(task => task.DataCriacao)
            .Select(task => new TaskResponse(
                task.Id,
                task.Titulo,
                task.Descricao,
                task.Status,
                task.DataCriacao))
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
    {
        var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(task => task.Id == id);

        if (task is null)
        {
            return NotFound();
        }

        return Ok(new TaskResponse(task.Id, task.Titulo, task.Descricao, task.Status, task.DataCriacao));
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request)
    {
        var normalizedStatus = NormalizeStatus(request.Status);

        if (normalizedStatus is null)
        {
            return BadRequest(new { message = "Status invalido. Use Pendente ou Concluida." });
        }

        var task = new TaskItem
        {
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            Status = normalizedStatus,
            DataCriacao = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var response = new TaskResponse(task.Id, task.Titulo, task.Descricao, task.Status, task.DataCriacao);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, UpdateTaskRequest request)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(item => item.Id == id);

        if (task is null)
        {
            return NotFound();
        }

        var normalizedStatus = NormalizeStatus(request.Status);

        if (normalizedStatus is null)
        {
            return BadRequest(new { message = "Status invalido. Use Pendente ou Concluida." });
        }

        task.Titulo = request.Titulo.Trim();
        task.Descricao = request.Descricao.Trim();
        task.Status = normalizedStatus;

        await _context.SaveChangesAsync();

        return Ok(new TaskResponse(task.Id, task.Titulo, task.Descricao, task.Status, task.DataCriacao));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(item => item.Id == id);

        if (task is null)
        {
            return NotFound();
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string? NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return AllowedStatus.FirstOrDefault(allowed =>
            string.Equals(allowed, status.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
