using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public interface IWrite<T,TKey>
    {
        T Write(T entity);
        T Update(TKey id, T entity);
        T Delete(TKey id);
    }
}
