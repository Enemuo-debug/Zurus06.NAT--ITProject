using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.dtos;
using server.NATModels;
using server.tools;

namespace server.Interfaces
{
    public interface IContent
    {
        Task<NATContent?> CreateNewContent(NewContentDto newContentDto, string userId, string url = null);
        Task<NATContent?> GetContentById(int contentId);
        Task<OutputContentGroup?> GetContentById(int contentId, ISimulation nS);
        Task<bool> UpdateContent(NATContent updatedContent, int contentId, bool save);
        Task<Tuple<int, bool>> DeleteContent(int contentId, bool save);
        Task<bool> ClearUnusedContents(string userId);
    }
}