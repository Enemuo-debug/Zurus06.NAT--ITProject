using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.NATModels;
using server.dtos;
using server.tools;
using System.Security.Claims;

namespace server.Interfaces
{
    public interface IPosts
    {
        Task<List<NATPosts>> GetAllPosts();
        Task<NATPosts?> CreateANewPost(NewPostDto newPost, string email);
        Task<NATPosts?> GetPostById(int postId);
        Task<List<OutputPostDto>> GetAllUsersPosts(string userId);
        Task<bool> UpdatePost(UpdatePostDto updatedPost, int postId, string email);
        Task<NATUser?> GetLoggedInUser(ClaimsPrincipal claimsPrincipal);
        Task<bool> DeletePost(int postId, string userId);
        Task<NATComments?> CreateComment(int postId, string message, string userId);
        Task<List<NATComments>> GetCommentsForPost(int postId);
    }
}