using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SocialMediaAppLibrary.Validation
{
    /// <summary>
    /// Validates that an uploaded <see cref="IFormFile"/> or collection of files does not exceed a maximum size in bytes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class MaxFileSizeAttribute2: ValidationAttribute
    {
        /// <summary>
        /// The maximum allowed file size in bytes.
        /// </summary>
        private readonly long _maxSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxFileSizeAttribute2"/> class with the specified maximum file size.
        /// </summary>
        /// <param name="maxSize">The maximum allowed file size in bytes.</param>
        public MaxFileSizeAttribute2(long maxSize):base($"Filen må ikke være større end 5 MB.")
        {
            _maxSize = maxSize;
        }

        /// <summary>
        /// Determines whether the specified file or collection of files is within the allowed maximum size.
        /// </summary>
        /// <param name="value">The value to validate, expected to be an <see cref="IFormFile"/> or <see cref="IEnumerable{T}"/> of <see cref="IFormFile"/>.</param>
        /// <returns><see langword="true"/> if the value is <see langword="null"/> or every file is within the size limit; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is neither <see langword="null"/>, an <see cref="IFormFile"/>, nor an <see cref="IEnumerable{T}"/> of <see cref="IFormFile"/>.</exception>
        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
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
                    return false;
                }
            }

            return true;
        }
    }
}
