using Microsoft.EntityFrameworkCore;
using server.Interfaces;
using server.data;
using server.NATModels;
using server.dtos;
using server.tools;
using server.MappersAndExtensions;
using System.Security.Claims;

namespace server.Repositories
{
    public class PostsRepo : IPosts
    {
        private readonly ApplicationDbContext context;
        private readonly IContent contentRepo;

        public PostsRepo(ApplicationDbContext _context, IContent _contentRepo)
        {
            context = _context;
            contentRepo = _contentRepo;
        }

        public async Task<List<NATPosts>> GetAllPosts()
        {
            return await context.Posts.ToListAsync();
        }

        public async Task<NATPosts?> CreateANewPost(NewPostDto newPost, string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            var post = new NATPosts
            {
                userId = user.Id,
                Caption = newPost.Caption,
                Intro = newPost.Intro
            };

            await context.Posts.AddAsync(post);
            await context.SaveChangesAsync();

            return post;
        }
        public async Task<NATPosts?> GetPostById(int postId)
        {
            return await context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<List<OutputPostDto>> GetAllUsersPosts(string userId)
        {
            var posts = await context.Posts.Where(p => p.userId == userId).ToListAsync();
            List<OutputPostDto> allPosts = new List<OutputPostDto>();
            foreach (var post in posts)
            {
                allPosts.Add(await post.PostDetails(contentRepo));
            }
            return allPosts;
        }

        public async Task<bool> UpdatePost(UpdatePostDto updatedPost, int postId, string? email)
        {
            NATPosts? post = await GetPostById(postId);
            if (post == null) return false;

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || post.userId != user.Id) return false;

            post.Caption = updatedPost.Caption;
            post.Intro = updatedPost.Intro;

            List<int> existingContentIds = await PostsExtension.GetContentIdsOnAPost(post, contentRepo);
            List<int>? newContentsArray = updatedPost.Contents;

            if (newContentsArray != null)
            {
                // Remove deleted contents
                foreach (var contentId in existingContentIds.ToList())
                {
                    if (!newContentsArray.Contains(contentId))
                    {
                        await contentRepo.DeleteContent(contentId, false);
                    }
                }

                // Link the new chain of contents
                if (newContentsArray.Count > 0)
                {
                    post.Content = newContentsArray[0];

                    for (int i = 0; i < newContentsArray.Count; i++)
                    {
                        var currentContent = await contentRepo.GetContentById(newContentsArray[i]);
                        var nextContent = (i < newContentsArray.Count - 1)
                            ? await contentRepo.GetContentById(newContentsArray[i + 1])
                            : null;

                        if (currentContent != null && nextContent != null) currentContent.Link = nextContent.Id;
                    }
                }
            }

            // ✅ Save changes even if no contents were provided
            context.Posts.Update(post);
            await context.SaveChangesAsync();

            // Optionally clean up unused contents after updating
            await contentRepo.ClearUnusedContents(user.Id);

            return true;
        }

        public async Task<NATUser?> GetLoggedInUser(ClaimsPrincipal claimsPrincipal)
        {
            return await claimsPrincipal.GetUser(context);
        }

        public async Task<bool> DeletePost(int postId, string userId)
        {
            var post = await context.Posts.FindAsync(postId);
            bool answer = false;
            if (post == null) return answer;
            // Delets all associated contents
            int current = post.Content;
            while (current != 0)
            {
                Tuple<int, bool> isDeleted = await contentRepo.DeleteContent(current, false);
                current = isDeleted.Item1;
            }
            // Delete Post and unused content
            context.Posts.Remove(post);
            bool result = await contentRepo.ClearUnusedContents(userId);
            while (!result)
            {
                try
                {
                    context.Posts.Remove(post);
                    result = await contentRepo.ClearUnusedContents(userId);
                }
                catch (System.Exception)
                {
                    continue;
                }
            }

            return result;
        }
    }
}
