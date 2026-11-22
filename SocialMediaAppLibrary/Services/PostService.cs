using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }
       
        public Task<Post> WriteAsync(Post entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<Post> UpdateAsync(int id, Post entity)
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




        //NEDENSTÅENDE SLETTES NÅR ALLE RPEOS ER OVERGÅET TIL TASK

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

        
        public Post Delete(int id)
        {
            throw new NotImplementedException();
        }
        public List<Post> ReadAll()
        {
            throw new NotImplementedException();
        }
    }
}
