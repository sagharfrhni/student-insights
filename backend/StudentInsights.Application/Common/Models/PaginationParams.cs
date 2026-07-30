namespace StudentInsights.Application.Common.Models;

/// <summary>
/// Shared paging input for all list queries (Courses, LearningActivities,
/// Exams, Goals, etc.). Invalid values are clamped to safe defaults instead
/// of throwing exceptions. FluentValidation remains the primary mechanism
/// for returning validation errors; this class acts as a defensive fallback
/// to keep paging behavior predictable.
/// </summary>
public class PaginationParams
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    private int _pageNumber = DefaultPageNumber;
    private int _pageSize = DefaultPageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < DefaultPageNumber
            ? DefaultPageNumber
            : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }
}