using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IWrite<T,TKey>
    {
        public T Write(T entity);
        public T Update(TKey id, T entity);
        public T Delete(TKey id);
        public Task<T> WriteAsync(T entity, CancellationToken cancellationToken);
    }
}
