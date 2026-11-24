using System.ComponentModel.DataAnnotations;

/// <summary>
/// Validates that a string value, after removing leading and trailing white-space characters,
/// has a length within a specified minimum and maximum range.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class TrimmedStringLengthAttribute : ValidationAttribute
{
    private readonly int _minLength;
    private readonly int _maxLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrimmedStringLengthAttribute"/> class.
    /// </summary>
    /// <param name="maxLength">The maximum allowed length of the trimmed string.</param>
    /// <param name="minLength">The minimum allowed length of the trimmed string. Defaults to <c>1</c>.</param>
    public TrimmedStringLengthAttribute(int minLength, int maxLength)
        : base($"Feltet skal være mellem {minLength} og {maxLength} tegn efter fjernelse af mellemrum.")
    {
        _minLength = minLength;
        _maxLength = maxLength;
    }

    /// <summary>
    /// Determines whether the specified value is a string whose trimmed length falls between
    /// <see cref="_minLength"/> and <see cref="_maxLength"/> inclusive.
    /// </summary>
    /// <param name="value">The value of the object to validate.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> is a <see cref="string"/> whose trimmed length
    /// is within the allowed range; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsValid(object? value)
    {
        if (value is not string str)
        {
            return false;
        }

        string trimmed = str.Trim();
        return trimmed.Length >= _minLength && trimmed.Length <= _maxLength;
    }
}