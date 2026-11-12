using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public interface IRead<T,TKey>
    {
        public T? ReadById(TKey id);
        public List<T> ReadAll();
    }
}
