using Microsoft.AspNetCore.Http;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IImageService
    {
        public Task<Image> GetByIdAsync(int id);
        public List<Image> GetAllImages();
        public Task<Image> SaveImageAsync(IFormFile file, string altText);
    }
}
