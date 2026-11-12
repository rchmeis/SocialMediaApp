using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;

namespace SocialMediaAppLibrary.Repository
{
    public class PostRepository : ICrud<Post, int>
    {

        private readonly DbConnect _dbConnect;

        public PostRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        Post IWrite<Post, int>.Delete(int id)
        {
            throw new NotImplementedException();
        }

        List<Post> IRead<Post, int>.ReadAll()
        {
            throw new NotImplementedException();
        }

        Post? IRead<Post, int>.ReadById(int id)
        {
            throw new NotImplementedException();
        }

        Post IWrite<Post, int>.Update(int id, Post entity)
        {
            throw new NotImplementedException();
        }

        Post IWrite<Post, int>.Write(Post entity)
        {
            throw new NotImplementedException();
        }
    }
}
