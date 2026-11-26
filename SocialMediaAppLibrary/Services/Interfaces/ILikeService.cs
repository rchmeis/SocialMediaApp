using System;
using SocialMediaAppLibrary.Models;

namespace SocialMediaAppLibrary.Services.Interfaces;

public interface ILikeService
{
    public Task<bool> IsLikedAsync(PostLike key, CancellationToken cancellationToken);
    public Task LikeAsync(PostLike key, CancellationToken cancellationToken);
    public Task UnlikeAsync(PostLike key, CancellationToken cancellationToken);
}
