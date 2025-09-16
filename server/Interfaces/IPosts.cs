using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.NATModels;
using server.dtos;

namespace server.Interfaces
{
    public interface IPosts
    {
        Task<List<NATPosts>> GetAllPosts();
        Task<NATPosts?> CreateANewPost(NewPostDto newPost, string email);
        Task<NATPosts?> GetPostById(int postId);
        Task<bool> GetAllUsersPosts(string userId);
    }
}