using Microsoft.AspNetCore.Http;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using ImageSharp = SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SixLabors.ImageSharp.Formats.Webp;

namespace SocialMediaAppLibrary.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;

        public ImageService(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public async Task<List<Image>> GetAllAsync()
        {
            return _imageRepository.ReadAll();
        }

        /// <summary>
        /// Retrieves an image by its ID from the repository.
        /// </summary>
        /// <param name="id">The ID of the image to retrieve.</param>
        /// <returns>The image with the specified ID, or null if not found.</returns>
        public async Task<Image?> GetByIdAsync(int id)
        {
            return _imageRepository.ReadById(id);
        }

        /// <summary>
        /// Saves an image file to the repository after optimizing it to meet the file size requirement of 64 KB or less.
        /// </summary>
        /// <param name="file">The image file to save.</param>
        /// <param name="altText">The alternative text for the image.</param>
        /// <returns>The saved image.</returns>
        public async Task<Image> SaveImageAsync(IFormFile file, string altText)
        {
            Image optimizedImage = await OptimizeImageAsync(file);

            optimizedImage.AltText = altText; // Set the alt text for the image

            return _imageRepository.Write(optimizedImage);
        }

        /// <summary>
        /// Optimizes the image to ensure it meets the file size requirement of 64 KB or less.
        /// It attempts to reduce the quality of the image optimization in steps until the file size is acceptable.
        /// </summary>
        /// <param name="file">The image file to optimize.</param>
        /// <returns>The optimized image.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the image cannot be optimized to meet the file size requirements.</exception>
        public async Task<Image> OptimizeImageAsync(IFormFile file)
        {
            const int maxFileSize = 64 * 1024; // 64 KB
            const int minQuality = 10;
            const int maxQuality = 75;
            const int qualityStep = 5;

            using ImageSharp.Image image = await ImageSharp.Image.LoadAsync(file.OpenReadStream());

            // Resize image if it exceeds maximum dimensions while maintaining aspect ratio.
            if (image.Width > 1200 || image.Height > 1600)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new ImageSharp.Size(1200, 1600),
                    Mode = ResizeMode.Max
                }));
            }

            using MemoryStream output = new MemoryStream();

            byte[] imageData = [];

            for (int quality = maxQuality; quality >= minQuality; quality -= qualityStep)
            {
                output.SetLength(0); // Reset the output stream every iteration
                output.Position = 0;

                await image.SaveAsync(output, new WebpEncoder
                {
                    Quality = quality
                });

                imageData = output.ToArray();

                if (imageData.Length <= maxFileSize)
                {
                    return new Image()
                    {
                        FileName = Guid.NewGuid().ToString(), // Generate a unique filename and to obscure the user's original filename for privacy.
                        AltText = string.Empty, // Alt text can be set later when saving the image
                        ImageData = imageData,
                        Width = image.Width,
                        Height = image.Height,
                        FileSize = imageData.Length
                    };
                }
            }

            throw new InvalidOperationException("Could not optimize image to meet file size requirements.");
        }
    }
}
