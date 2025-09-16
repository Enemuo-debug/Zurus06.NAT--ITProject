using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.dtos;
using server.NATModels;
using server.tools;

namespace server.MappersAndExtensions
{
    public static class PostsExtension
    {
        public static OutputPostDto PostDetails(this NATPosts post)
        {
            return new OutputPostDto
            {
                Caption = Cipher.HillCipherDecrypt(post.Caption),
                Intro = Cipher.HillCipherDecrypt(post.Intro)
            };
        }

        public static NewPostDto EncryptPostDto(this NewPostDto postDto)
        {
            postDto.Caption = Cipher.HillCipherEncrypt(postDto.Caption);
            postDto.Intro = Cipher.HillCipherEncrypt(postDto.Intro);
            return postDto;
        }
        public static NewPostDto DecryptPostDto(this NewPostDto postDto)
        {
            postDto.Caption = Cipher.HillCipherEncrypt(postDto.Caption);
            postDto.Intro = Cipher.HillCipherEncrypt(postDto.Intro);
            return postDto;
        }
    }
}