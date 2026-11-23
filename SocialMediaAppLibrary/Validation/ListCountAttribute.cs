using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaAppLibrary.Validation;

public sealed class ListCountAttribute : ValidationAttribute
{
    /// <summary>
    /// The minimum number of items required in the list.
    /// </summary>
    private readonly int _min;

    /// <summary>
    /// The maximum number of items allowed in the list.
    /// </summary>
    private readonly int _max;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListCountAttribute"/> class with the specified minimum and maximum count.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public ListCountAttribute(int min, int max)
    {
        _min = min;
        _max = max;
    }

    /// <summary>
    /// Determines whether the specified value is valid based on the minimum and maximum count constraints.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns><c>true</c> if the specified value is valid; otherwise, <c>false</c>.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not ICollection collection)
        {
            return ValidationResult.Success;
        }

        if (collection.Count < _min || collection.Count > _max)
        {
            return new ValidationResult($"The list must contain between {_min} and {_max} items.");
        }

        return ValidationResult.Success;
    }
}
