using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly IDbConnect _dbConnect;

        public ImageRepository(IDbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public Image Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Image> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Image? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Image Update(int id, Image entity)
        {
            throw new NotImplementedException();
        }

        public Image Write(Image entity)
        {
            throw new NotImplementedException();
        }
    }
}
