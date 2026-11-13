using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
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
