using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.dtos;
using server.Interfaces;
using server.NATModels;
using server.tools;

namespace server.MappersAndExtensions
{
    public static class ContentExtensions
    {
        public static NewContentDto EncryptContentDto(this NewContentDto contentDto)
        {
            contentDto.Content = Cipher.HillCipherEncrypt(contentDto.Content);
            return contentDto;
        }

        public static async Task<OutputContentGroup> DecryptContentDto(this NATContent contentDto, ISimulation nS)
        {
            switch (contentDto.type)
            {
                case ContentTypes.Text:
                    return new TextContent
                    {
                        Id = contentDto.Id,
                        Content = Cipher.HillCipherDecrypt(contentDto.Content)
                    };

                case ContentTypes.Image:
                    if (string.IsNullOrEmpty(contentDto.Content))
                        throw new FormatException("Invalid content format for Image type: missing encrypted caption or data.");

                    return new ImageContent
                    {
                        Id = contentDto.Id,
                        ImgLink = contentDto.ImgLink,
                        Content = Cipher.HillCipherDecrypt(contentDto.Content)
                    };

                case ContentTypes.NATSimulation:
                    if (nS == null)
                        throw new ArgumentNullException(nameof(nS), "Simulation service cannot be null for NATSimulation content.");

                    return new NetContent
                    {
                        Id = contentDto.Id,
                        NATSimulation = null // Simulation fetching logic can be implemented here if needed
                    };

                default:
                    throw new NotSupportedException($"Unsupported content type: {contentDto.type}");
            }
        }
    }
}