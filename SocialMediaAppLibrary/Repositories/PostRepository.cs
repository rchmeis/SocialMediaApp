using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMediaAppLibrary.Models;

namespace SocialMediaAppLibrary.Repository
{
    public class PostRepository : ICrud<Post, int>
    {

        private readonly DbConnect _dbConnect;

        public PostRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

    }
}
