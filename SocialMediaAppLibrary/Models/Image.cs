using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Image
    {
        /// <summary>
        /// Gets or sets the unique identifier for the image.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the file name of the image.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the alternative text for the image.
        /// </summary>
        public string? AltText { get; set; }

        /// <summary>
        /// Gets or sets the binary data of the image.
        /// </summary>
        public byte[]? ImageData { get; set; }

        /// <summary>
        /// Gets or sets the height of the image.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the file size of the image.
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the image was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        public Image() { }

        public override string ToString()
        {
            return $"Image [Id={Id}, FileName={FileName}, AltText={AltText}, CreatedAt={CreatedAt}, Height={Height}, Width={Width}, FileSize={FileSize}]";
        }
    }
}
