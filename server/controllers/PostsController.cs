using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.Repositories;
using server.Interfaces;
using server.NATModels;
using server.dtos;
using Microsoft.AspNetCore.Mvc;
using server.MappersAndExtensions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using server.tools;

namespace server.controllers
{
    [ApiController]
    [Route("posts")]
    public class PostsController : ControllerBase
    {
        private readonly IPosts postsRepo;
        private readonly IContent contentRepo;
        private readonly FileUpload cloudinaryService;

        public PostsController(IPosts _postsRepo, IContent _contentRepo, FileUpload _cloudinaryService)
        {
            postsRepo = _postsRepo;
            contentRepo = _contentRepo;
            cloudinaryService = _cloudinaryService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPosts() 
        {
            var posts = await postsRepo.GetAllPosts();
            return Ok(posts.Select(p => p.PostDetails()));
        }

        [Authorize]
        [HttpPost("new")]
        public async Task<IActionResult> CreateANewPost([FromBody] NewPostDto newPost)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var email = User.GetUserEmail() ?? "";
            var post = await postsRepo.CreateANewPost(newPost.EncryptPostDto(), email);
            if (post == null) return BadRequest("Could not create post");
            return Ok(post.PostDetails());
        }

        [Authorize]
        [HttpPost("new-content")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePostContent([FromForm] NewContentDto newContent)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var file = newContent.File;
            string url = string.Empty;
            if (file != null && file.Length != 0)
            {
                url = await cloudinaryService.UploadImageAsync(file);
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest("File upload failed");
                }
            }
            var content = await contentRepo.CreateNewContent(newContent, url);
            if (content == null) return BadRequest("Could not create content");
            return Ok(content);
        }
    }
}
