using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IWrite<T,TKey>
    {
        public Task<T> WriteAsync(T entity, CancellationToken cancellationToken);
        public Task<T> UpdateAsync(TKey id, T entity, CancellationToken cancellationToken);
        public Task<T> DeleteAsync(TKey id, CancellationToken cancellationToken);
    }
}
