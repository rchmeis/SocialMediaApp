using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;

namespace SocialMediaAppLibrary.Repositories
{
    public class PostRepository : IPostRepository
    {

        private readonly IDbConnect _dbConnect;

        public PostRepository(IDbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public Task<Post> WriteAsync(Post entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<Post> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Post>> ReadAllAsync()
        {
            throw new NotImplementedException();
        }
       
        public Task<Post?> ReadByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Post> UpdateAsync(int id, Post entity)
        {
            throw new NotImplementedException();
        }



        //BRUG IKKE SYNKRONE METODER - fjernes når alle repos er skiftet til task
        public Post Write(Post entity)
        {
            throw new NotImplementedException();
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

    }
}
