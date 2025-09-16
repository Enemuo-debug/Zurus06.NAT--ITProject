using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.dtos;
using server.NATModels;

namespace server.Interfaces
{
    public interface IContent
    {
        Task<NATContent?> CreateNewContent(NewContentDto newContent, string url = null);
        // Task<NewContentDto?> GetContentsOfAPost(int postId);
        // Task<bool> DeleteContent(int contentId);
        // Task<bool> UpdateContent(int contentId, NewContentDto updatedContent);
    }
}