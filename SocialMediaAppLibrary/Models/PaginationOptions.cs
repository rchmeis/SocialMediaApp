using System;

namespace SocialMediaAppLibrary.Models;

/// <summary>
/// Represents the query options for fetching list of posts.
/// </summary>
public struct PaginationOptions
{
    /// <summary>
    /// Gets the page number for pagination.
    /// Must be non-negative.
    /// Immutable after initialization.
    /// Default value is 1.
    /// </summary>
    public int PageNumber { get; init; } = 1;
    /// <summary>
    /// Gets the page size for pagination.
    /// Must be non-negative.
    /// Immutable after initialization.
    /// Default value is 20.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginationOptions"/> struct with the specified page number and page size.
    /// </summary>
    /// <param name="pageNumber">The page number. Must be non-negative.</param>
    /// <param name="pageSize">The page size. Must be non-negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pageNumber"/> or <paramref name="pageSize"/> is negative.</exception>
    public PaginationOptions(int pageNumber, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pageNumber, nameof(pageNumber));
        ArgumentOutOfRangeException.ThrowIfNegative(pageSize, nameof(pageSize));

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public override string ToString()
    {
        return $"PageNumber: {PageNumber}, PageSize: {PageSize}";
    }
}
