using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMediaAppLibrary.Models;

namespace SocialMediaAppLibrary.Repositories
{
    public class PostRepository : IPostRepository
    {

        private readonly DbConnect _dbConnect;

        public PostRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public Post Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Post> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Post? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Post Update(int id, Post entity)
        {
            throw new NotImplementedException();
        }

        public Post Write(Post entity)
        {
            throw new NotImplementedException();
        }
    }
}
