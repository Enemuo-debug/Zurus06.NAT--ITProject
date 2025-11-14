using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.dtos;
using server.NATModels;
using server.tools;
using server.Repositories;
using server.MappersAndExtensions;
using server.Interfaces;

namespace server.MappersAndExtensions
{
    public static class PostsExtension
    {
        public async static Task<OutputPostDto> PostDetails(this NATPosts post, IContent contentRepo)
        {
            List<OutputContentGroup> allContents = new List<OutputContentGroup>();
            int currentContentId = post.Content;
            // Traverse linked contents like a linked list
            while (currentContentId != 0)
            {
                var content = await contentRepo.GetContentById(currentContentId);
                if (content == null) break;
                allContents.Add(await content.DecryptContentDto(null!));
                currentContentId = content.Link;
            }
            return new OutputPostDto
            {
                Id = post.Id,
                Caption = Cipher.HillCipherDecrypt(post.Caption),
                Intro = Cipher.HillCipherDecrypt(post.Intro),
                Contents = allContents,
                creatorId = post.userId
            };
        }

        public static NewPostDto EncryptPostDto(this NewPostDto postDto)
        {
            postDto.Caption = Cipher.HillCipherEncrypt(postDto.Caption);
            postDto.Intro = Cipher.HillCipherEncrypt(postDto.Intro);
            return postDto;
        }

        public static UpdatePostDto EncryptPostDto(this UpdatePostDto postDto)
        {
            postDto.Caption = Cipher.HillCipherEncrypt(postDto.Caption);
            postDto.Intro = Cipher.HillCipherEncrypt(postDto.Intro);
            return postDto;
        }

        public static NewPostDto DecryptPostDto(this NewPostDto postDto)
        {
            postDto.Caption = Cipher.HillCipherDecrypt(postDto.Caption);
            postDto.Intro = Cipher.HillCipherDecrypt(postDto.Intro);
            return postDto;
        }
        public static async Task<List<int>> GetContentIdsOnAPost(this NATPosts post, IContent contentRepo)
        {
            int currentContentId = post.Content;
            List<int> output = [];
            while (currentContentId > 0)
            {
                output.Add(currentContentId);
                var nextContent = await contentRepo.GetContentById(currentContentId);
                currentContentId = nextContent.Link;
            }
            return output;
        }
    }
}