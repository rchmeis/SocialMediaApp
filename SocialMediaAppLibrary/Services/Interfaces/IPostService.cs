using Microsoft.AspNetCore.Http;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IPostService
    {
        public Task<Post> CreateAsync(Post entity, List<IFormFile> formFiles, CancellationToken cancellationToken);
    }
}
