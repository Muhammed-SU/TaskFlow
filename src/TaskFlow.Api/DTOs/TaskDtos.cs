using System.ComponentModel.DataAnnotations;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.DTOs;

public class CreateTaskRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    public DateTime? DueDate { get; set; }
}

public class UpdateTaskRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public TodoStatus Status { get; set; }

    public TodoPriority Priority { get; set; }

    public DateTime? DueDate { get; set; }
}

public class TaskQuery
{
    public TodoStatus? Status { get; set; }
    public TodoPriority? Priority { get; set; }
    public string? Search { get; set; }

    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : Math.Min(value, 50);
    }
}

public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    TodoStatus Status,
    TodoPriority Priority,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
