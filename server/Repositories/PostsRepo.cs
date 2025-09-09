using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.Interfaces;
using server.data;
using server.NATModels;

namespace server.Repositories
{
    public class PostsRepo: IPosts
    {
        private readonly ApplicationDbContext context;
        public PostsRepo(ApplicationDbContext _context)
        {
            context = _context;
        }

        public Task<List<NATPosts>> GetAllPosts() 
        {
            context.Posts.
        }
        public Task<NATPosts> CreateANewPost()
        {
            
        }
        public Task<NATPosts> GetPostById()
        {

        }
        public Task DeletePostComponent() 
        {

        }
        public Task RecursiveDeletePosts()
        {

        }
    }
}