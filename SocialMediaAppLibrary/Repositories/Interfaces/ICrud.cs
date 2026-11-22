using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface ICrud<T, TKey> : IRead<T, TKey>, IWrite<T, TKey>
    {
    }
}
