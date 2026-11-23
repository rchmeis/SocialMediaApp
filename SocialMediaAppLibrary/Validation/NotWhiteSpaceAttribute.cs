using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SocialMediaAppLibrary.Validation
{
    /// <summary>
    /// Specifies that a string value must not be <see langword="null"/>, empty, or consist solely of white-space characters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class NotWhiteSpaceAttribute:ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotWhiteSpaceAttribute"/> class with a default error message.
        /// </summary>
        public NotWhiteSpaceAttribute()
        : base("Feltet må ikke være tomt eller kun bestå af mellemrum.")
        {
        }

        /// <summary>
        /// Determines whether the specified value is a non-null string containing at least one non-white-space character.
        /// </summary>
        /// <param name="value">The value of the object to validate.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="value"/> is a <see cref="string"/> that is not null, empty, or white-space;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return false;
            }

            if (value is string str)
            {
                return !string.IsNullOrWhiteSpace(str);
            }

            return false;
        }
    }
}
