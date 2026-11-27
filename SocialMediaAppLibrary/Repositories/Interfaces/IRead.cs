using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IRead<T,TKey>
    {
        public Task<T?> ReadByIdAsync(TKey id, CancellationToken cancellationToken);
        public Task<List<T>> ReadAllAsync(CancellationToken cancellationToken);
    }
}
