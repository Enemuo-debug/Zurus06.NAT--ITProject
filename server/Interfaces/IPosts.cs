using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.NATModels;

namespace server.Interfaces
{
    public interface IPosts
    {
        Task<List<NATPosts>> GetAllPosts();
        Task<NATPosts> CreateANewPost();
        Task<NATPosts> GetPostById();
        Task DeletePostComponent();
        Task RecursiveDeletePosts();
    }
}