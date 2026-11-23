using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SocialMediaAppLibrary.Validation;

public sealed class MaxFileSizeAttribute : ValidationAttribute
{
    /// <summary>
    /// The maximum allowed file size in bytes.
    /// </summary>
    private readonly long _maxSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxFileSizeAttribute"/> class with the specified maximum file size.
    /// </summary>
    /// <param name="maxSize">The maximum allowed file size in bytes.</param>
    public MaxFileSizeAttribute(long maxSize)
    {
        _maxSize = maxSize;
    }

    /// <summary>
    /// Determines whether the specified value is valid based on the maximum file size constraint.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns><c>true</c> if the specified value is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is not of the expected type.</exception>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        IEnumerable<IFormFile> files = value switch
        {
            IFormFile file => [file],
            IEnumerable<IFormFile> collection => collection,
            _ => throw new ArgumentException("Attribute can only be applied to IFormFile or collections of IFormFile.")
        };

        foreach (IFormFile file in files)
        {
            if (file.Length > _maxSize)
            {
                return new ValidationResult(ErrorMessage ?? $"File '{file.FileName}' exceeds the maximum allowed size of {_maxSize / 1024.0 / 1024.0:N1} MB.");
            }
        }

        return ValidationResult.Success;
    }
}
