using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using server.Interfaces;
using server.data;
using server.NATModels;
using server.dtos;

namespace server.Repositories
{
    public class PostsRepo : IPosts
    {
        private readonly ApplicationDbContext context;

        public PostsRepo(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task<List<NATPosts>> GetAllPosts()
        {
            return await context.Posts.ToListAsync();
        }

        public async Task<NATPosts?> CreateANewPost(NewPostDto newPost, string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;
            if (newPost.Contents != null && newPost.Contents.Count > 0)
            {
                List<NATContent> allContents = new List<NATContent>();
                for (int i = 0; i < newPost.Contents.Count; i++)
                {
                    var cont = await context.Contents.FirstOrDefaultAsync(c => c.Id == newPost.Contents[i]);
                    if (cont == null) return null;
                    allContents.Add(cont);
                }
                for (int i = 0; i < allContents.Count; i++)
                {
                    allContents[i].Link = i < allContents.Count - 1 ? allContents[i + 1].Id : 0;
                    context.Contents.Update(allContents[i]);
                    await context.SaveChangesAsync();
                }
            }

            var post = new NATPosts
            {
                userId = user.Id,
                Caption = newPost.Caption,
                Intro = newPost.Intro,
                Content = newPost.Contents != null && newPost.Contents.Count > 0 ? newPost.Contents[0] : 0
            };

            context.Posts.Add(post);
            await context.SaveChangesAsync();

            return post;
        }
        public async Task<NATPosts?> GetPostById(int postId)
        {
            return await context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<bool> GetAllUsersPosts(string userId)
        {
            var posts = await context.Posts.Where(p => p.userId == userId).ToListAsync();
            return posts.Any();
        }
    }
}
