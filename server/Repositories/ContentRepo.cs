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

        public async Task<NATContent?> CreateNewContent(NewContentDto newContentDto, string userId, string url = "")
        {
            newContentDto = newContentDto.EncryptContentDto();
            if (newContentDto.type == ContentTypes.Text.ToString())
            {
                var newContent = new NATContent
                {
                    type = Enum.Parse<ContentTypes>(newContentDto.type),
                    Content = newContentDto.Content,
                    Owner = userId
                };
                await context.Contents.AddAsync(newContent);
                await context.SaveChangesAsync();
                return newContent;
            }
            else if (newContentDto.type == ContentTypes.Image.ToString())
            {
                var newContent = new NATContent
                {
                    type = Enum.Parse<ContentTypes>(newContentDto.type),
                    Content = newContentDto.Content,
                    ImgLink = url,
                    Owner = userId
                };
                await context.Contents.AddAsync(newContent);
                await context.SaveChangesAsync();
                return newContent;
            }
            else if (newContentDto.type == ContentTypes.NATSimulation.ToString())
            {
                var newContent = new NATContent
                {
                    type = Enum.Parse<ContentTypes>(newContentDto.type),
                    simUUID = newContentDto.simUUID,
                    Owner = userId
                };
                await context.Contents.AddAsync(newContent);
                await context.SaveChangesAsync();
                return newContent;
            }
            else
            {
                return null;
            }
        }

        public async Task<NATContent?> GetContentById(int contentId)
        {
            return await context.Contents.FindAsync(contentId);
        }
        public async Task<OutputContentGroup?> GetContentById(int contentId, ISimulation nS)
        {
            var output = await context.Contents.FindAsync(contentId);
            if (output == null) return null;
            return await output!.DecryptContentDto(nS);
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
            var contents = await context.Contents
                .Where(c => c.Owner == userId)
                .ToListAsync();

            if (contents.Count == 0)
                return true;

            for (int i = 0; i < contents.Count; i++)
            {
                var content = contents[i];

                // Step 3: Check if this content is linked to any post or content
                bool isLinked = await context.Posts.AnyAsync(p => p.Content == content.Id) || await context.Contents.AnyAsync(c => c.Link == content.Id);
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