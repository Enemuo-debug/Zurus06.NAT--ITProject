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
        private readonly ISimulation simRepo;

        public PostsRepo(ApplicationDbContext _context, IContent _contentRepo, ISimulation _simRepo)
        {
            context = _context;
            contentRepo = _contentRepo;
            simRepo = _simRepo;
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
                allPosts.Add(await post.PostDetails(contentRepo, simRepo));
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
            List<int> newContentsArray = updatedPost.Contents;

            if (newContentsArray != null)
            {
                // Remove deleted contents
                foreach (int contentId in existingContentIds)
                {
                    if (!newContentsArray.Contains(contentId))
                    {
                        // Ensure content is not referenced by any other post or content before deleting
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
                        var nextContentId = (i < newContentsArray.Count - 1) ? newContentsArray[i + 1] : 0;
                        if (currentContent != null)
                        {
                            currentContent.Link = nextContentId;
                            currentContent.Owner = user.Id; // Ensure the content owner is correct
                            currentContent.linked = true;
                        }
                    }
                }
                else
                {
                    // If no contents, unlink the post
                    post.Content = 0;
                }
            }
            else
            {
                // If contents array is null, unlink the post
                post.Content = 0;
            }

            context.Posts.Update(post);
            await context.SaveChangesAsync();

            // Clean up unused contents after updating
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
            if (post == null) return false;

            // Delete all linked contents (if any)
            int current = post.Content;
            while (current != 0)
            {
                var isDeleted = await contentRepo.DeleteContent(current, false);
                current = isDeleted.Item1;
            }

            // Delete the post
            context.Posts.Remove(post);
            await context.SaveChangesAsync();

            // Clean up orphaned contents
            await contentRepo.ClearUnusedContents(userId);

            return true;
        }

        public async Task<NATComments?> CreateComment(int postId, string message, string userId)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(message)) return null;

            // Ensure post exists
            var post = await GetPostById(postId);
            if (post == null) return null;

            // Ensure user exists
            var user = await context.Users.FindAsync(userId);
            if (user == null) return null;

            var comment = new NATComments
            {
                PostId = postId,
                UserId = userId,
                Message = message.Trim()
            };

            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();

            return comment;
        }

        public async Task<List<NATComments>> GetCommentsForPost(int postId)
        {
            var comments = await context.Comments
                .Where(c => c.PostId == postId)
                .ToListAsync();
            return comments;
        }
    }
}
