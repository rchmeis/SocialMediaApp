using System;

namespace SocialMediaAppLibrary.Models;

public class GenericPaginatedResult<T>
{
    /// <summary>
    /// Gets the list of items in the current page of the paginated result.
    /// </summary>
    public List<T> Items { get; init; } = new List<T>();

    /// <summary>
    /// Gets a value indicating whether there are more items available beyond the current page.
    /// </summary>
    public bool HasMore { get; init; }

    /// <summary>
    /// Gets the total count of items across all pages in the paginated result.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the current page number in the paginated result.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets the number of items per page in the paginated result.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericPaginatedResult{T}"/> class.
    /// </summary>
    public GenericPaginatedResult() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericPaginatedResult{T}"/> class with the specified items, pagination information, and total count.
    /// </summary>
    /// <param name="items">The list of items in the current page.</param>
    /// <param name="hasMore">A value indicating whether there are more items available beyond the current page.</param>
    /// <param name="totalCount">The total count of items across all pages in the paginated result.</param>
    /// <param name="pageNumber">The current page number in the paginated result.</param>
    /// <param name="pageSize">The number of items per page in the paginated result.</param>
    public GenericPaginatedResult(List<T> items, bool hasMore, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        HasMore = hasMore;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public override string ToString()
    {
        return $"GenericPaginatedResult<{typeof(T).Name}>: Page {PageNumber} of {Math.Ceiling((double)TotalCount / PageSize)}, TotalCount: {TotalCount}, HasMore: {HasMore}";
    }
}
