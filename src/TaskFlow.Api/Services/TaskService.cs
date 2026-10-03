using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Exceptions;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public interface ITaskService
{
    Task<PagedResult<TaskResponse>> GetAllAsync(int userId, TaskQuery query);
    Task<TaskResponse> GetByIdAsync(int userId, int taskId);
    Task<TaskResponse> CreateAsync(int userId, CreateTaskRequest request);
    Task<TaskResponse> UpdateAsync(int userId, int taskId, UpdateTaskRequest request);
    Task DeleteAsync(int userId, int taskId);
}

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;

    public TaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<TaskResponse>> GetAllAsync(int userId, TaskQuery query)
    {
        // Every query is scoped to the current user, so users can never see each other's tasks
        var tasks = _db.Tasks.AsNoTracking().Where(t => t.UserId == userId);

        if (query.Status.HasValue)
            tasks = tasks.Where(t => t.Status == query.Status.Value);

        if (query.Priority.HasValue)
            tasks = tasks.Where(t => t.Priority == query.Priority.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            tasks = tasks.Where(t => t.Title.Contains(term));
        }

        var totalCount = await tasks.CountAsync();

        var items = await tasks
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<TaskResponse>(
            items.Select(ToResponse).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    public async Task<TaskResponse> GetByIdAsync(int userId, int taskId)
    {
        var task = await FindOwnedTaskAsync(userId, taskId);
        return ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(int userId, CreateTaskRequest request)
    {
        var task = new TodoItem
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            DueDate = request.DueDate
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return ToResponse(task);
    }

    public async Task<TaskResponse> UpdateAsync(int userId, int taskId, UpdateTaskRequest request)
    {
        var task = await FindOwnedTaskAsync(userId, taskId);

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToResponse(task);
    }

    public async Task DeleteAsync(int userId, int taskId)
    {
        var task = await FindOwnedTaskAsync(userId, taskId);
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
    }

    private async Task<TodoItem> FindOwnedTaskAsync(int userId, int taskId)
    {
        // Returning 404 (not 403) for other users' tasks avoids revealing that the task exists
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
        return task ?? throw new NotFoundException($"Task with id {taskId} was not found.");
    }

    private static TaskResponse ToResponse(TodoItem t) =>
        new(t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate, t.CreatedAt, t.UpdatedAt);
}
