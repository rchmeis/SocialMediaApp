using Microsoft.AspNetCore.Http;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IImageService
    {
        public Task<Image?> GetByIdAsync(int id, CancellationToken cancellationToken);
        public Task<List<Image>> GetAllAsync(CancellationToken cancellationToken);
        //denne bør hedder OptimizeAndSave
        public Task<Image> SaveImageAsync(IFormFile file, string altText, CancellationToken cancellationToken);
        public Task<Image> OptimizeImageAsync(IFormFile file);
        //denne gemmer uden yderligere handlinger - bruges feks i step2 onboarding, hvor billeder er optimeret før preview.
        public Task<Image> WriteImageAsync(Image image, CancellationToken cancellationToken);
    }
}
