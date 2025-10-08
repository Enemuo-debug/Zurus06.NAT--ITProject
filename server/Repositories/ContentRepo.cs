using server.Interfaces;
using server.dtos;
using server.NATModels;
using server.data;
using server.tools;
using server.MappersAndExtensions;
using Microsoft.EntityFrameworkCore;

namespace server.Repositories
{
    public class ContentRepo : IContent
    {
        private readonly ApplicationDbContext context;

        public ContentRepo(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task<NATContent?> CreateNewContent(NewContentDto newContentDto, string userId, string url = null)
        {
            newContentDto = newContentDto.EncryptContentDto();
            var newContent = new NATContent
            {
                type = Enum.Parse<ContentTypes>(newContentDto.type),
                ImgLink = url,
                Content = newContentDto.Content,
                Owner = userId
            };
            await context.Contents.AddAsync(newContent);
            await context.SaveChangesAsync();
            return newContent;
        }

        public async Task<NATContent?> GetContentById(int contentId)
        {
            return await context.Contents.FindAsync(contentId);
        }
        public async Task<OutputContentGroup?> GetContentById(int contentId, ISimulation nS)
        {
            var output = await context.Contents.FindAsync(contentId);
            return await output.DecryptContentDto(nS);
        }
        public async Task<bool> UpdateContent(NATContent updatedContent, int contentId, bool save)
        {
            NATContent? content = await GetContentById(contentId);
            if (content == null) return false;
            content.Content = Cipher.HillCipherEncrypt(updatedContent.Content);
            content.ImgLink = updatedContent.ImgLink;
            content.simUUID = updatedContent.simUUID;
            context.Contents.Update(content);
            if (save) await context.SaveChangesAsync();
            return true;
        }
        public async Task<Tuple<int, bool>> DeleteContent(int contentId, bool save)
        {
            NATContent? content = await GetContentById(contentId);
            if (content == null) return Tuple.Create(0, false);
            Tuple<int, bool> output = Tuple.Create(content.Link, true);
            context.Contents.Remove(content);
            if (save) await context.SaveChangesAsync();
            return output;
        }
        public async Task<bool> ClearUnusedContents(string userId)
        {
            // Step 1: Query database for matching contents
            var contents = await context.Contents
                .Where(c => c.Owner == userId)
                .ToListAsync();

            // Step 2: If nothing found, you can safely return
            if (contents.Count == 0)
                return true;

            for (int i = 0; i < contents.Count; i++)
            {
                var content = contents[i];

                // Step 3: Check if this content is linked to any post or content
                bool isLinked = await context.Posts.AnyAsync(p => p.Content == content.Id) ||
                                await context.Contents.AnyAsync(c => c.Link == content.Id);
                if (!isLinked)
                {
                    context.Contents.Remove(content);
                }
            }

            // Step 4: Save changes to database
            int result = await context.SaveChangesAsync();
            return result > 0;
        }
    }
}