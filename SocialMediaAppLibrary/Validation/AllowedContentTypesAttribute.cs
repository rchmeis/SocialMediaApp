using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SocialMediaAppLibrary.Validation;

public sealed class AllowedContentTypesAttribute : ValidationAttribute
{
    /// <summary>
    /// The allowed content types for the file(s) being validated.
    /// </summary>
    private readonly string[] _allowedContentTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedContentTypesAttribute"/> class with the specified allowed content types.
    /// </summary>
    /// <param name="allowedContentTypes">The allowed content types.</param>
    public AllowedContentTypesAttribute(params string[] allowedContentTypes)
    {
        _allowedContentTypes = allowedContentTypes;
    }

    /// <summary>
    /// Determines whether the specified value is valid based on the allowed content types.
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
            IFormFile file => new[] { file },
            IEnumerable<IFormFile> collection => collection,
            _ => throw new ArgumentException("Attribute can only be applied to IFormFile or collections of IFormFile.")
        };

        foreach (IFormFile file in files)
        {
            if (!_allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                return new ValidationResult(ErrorMessage ??
                    $"File '{file.FileName}' has an invalid content type. " +
                    $"Allowed types: {string.Join(", ", _allowedContentTypes)}"
                );
            }
        }

        return ValidationResult.Success;
    }
}
