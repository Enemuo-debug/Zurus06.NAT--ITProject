using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.Interfaces;
using server.dtos;
using server.NATModels;
using server.data;
using server.tools;

namespace server.Repositories
{
    public class ContentRepo: IContent
    {
        private readonly ApplicationDbContext context;

        public ContentRepo(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task<NATContent?> CreateNewContent(NewContentDto newContentDto, string url = null)
        {
            var newContent = new NATContent
            {
                type = ContentTypeFunctions.MapContentTypes(int.Parse(newContentDto.type)),
                ImgLink = url,
                Content = newContentDto.Content
            };
            await context.Contents.AddAsync(newContent);
            await context.SaveChangesAsync();
            return newContent;
        }
    }
}