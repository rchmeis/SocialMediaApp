using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface ILikeRepository
    {
        public Task<bool> IsLikedAsync(PostLike key, CancellationToken cancellationToken);
        public Task LikeAsync(PostLike key, CancellationToken cancellationToken);
        public Task UnlikeAsync(PostLike key, CancellationToken cancellationToken);
    }
}