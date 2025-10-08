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
using Microsoft.AspNetCore.Identity;
using server.data;

namespace server.controllers
{
    [ApiController]
    [Route("posts")]
    public class PostsController : ControllerBase
    {
        private readonly IPosts postsRepo;
        private readonly IContent contentRepo;
        private readonly FileUpload cloudinaryService;
        private readonly UserManager<NATUser> userManager;

        public PostsController(IPosts _postsRepo, IContent _contentRepo, FileUpload _cloudinaryService, UserManager<NATUser> _userManager)
        {
            postsRepo = _postsRepo;
            contentRepo = _contentRepo;
            cloudinaryService = _cloudinaryService;
            userManager = _userManager;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await postsRepo.GetAllPosts();
            var detailedPosts = new List<OutputPostDto>();
            foreach (var post in posts)
            {
                detailedPosts.Add(await post.PostDetails(contentRepo));
            }
            return Ok(detailedPosts);
        }

        [Authorize]
        [HttpPost("new")]
        public async Task<IActionResult> CreateANewPost([FromBody] NewPostDto newPost)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var email = User.GetUserEmail() ?? "";
            var post = await postsRepo.CreateANewPost(newPost.EncryptPostDto(), email);
            if (post == null) return BadRequest("Could not create post");
            return Ok(await post.PostDetails(contentRepo));
        }

        [Authorize]
        [HttpPost("new-content")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePostContent([FromForm] NewContentDto newContent)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var email = User.GetUserEmail() ?? "";
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized("User not found");
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
            var cipherContent = await contentRepo.CreateNewContent(newContent, user.Id, url);
            if (cipherContent == null) return BadRequest("Could not create content");
            var plainContent = await cipherContent.DecryptContentDto();
            return Ok(plainContent);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPostById([FromRoute] int id)
        {
            var post = await postsRepo.GetPostById(id);
            if (post == null) return NotFound("Post not found");
            return Ok(await post.PostDetails(contentRepo));
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsByUserId([FromRoute] string userId)
        {
            var user = await postsRepo.GetLoggedInUser(User);
            if (user == null) return BadRequest("This user no longer exists...");
            var posts = await postsRepo.GetAllUsersPosts(userId);
            return Ok(posts);
        }

        [Authorize]
        [HttpPut("edit/{postId:int}")]
        public async Task<IActionResult> EditPost([FromRoute] int postId, [FromBody] NewPostDto editPost)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Get the logged in user
            var user = await postsRepo.GetLoggedInUser(User);
            if (user == null) return BadRequest("This user no longer exists...");
            editPost = editPost.EncryptPostDto();
            string? email = User.GetUserEmail();
            bool isUpdated = await postsRepo.UpdatePost(editPost, postId, email);
            return isUpdated ? Ok("Post Successfully Updated!!!") : StatusCode(204, new { message = "The post is being modified by someone else at this time" });
        }

        [Authorize]
        [HttpPut("edit-content/{contentId:int}")]
        public async Task<IActionResult> EditContent([FromRoute] int contentId, [FromBody] EditContentDto updatedContentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await postsRepo.GetLoggedInUser(User);
            if (user == null)
                return BadRequest("This user no longer exists...");

            var content = await contentRepo.GetContentById(contentId);
            if (content == null || content.Owner == user.Id)
                return BadRequest("The referenced content either doesn't exist or is not yours");

            // Now we know type is valid
            var type = Enum.Parse<ContentTypes>(updatedContentDto.type, true);

            if (type == ContentTypes.Image || content.type == ContentTypes.Image)
                return BadRequest("Image contents cannot be edited, only deleted");

            // I think you meant "must be the same type"
            if (content.type != type)
                return BadRequest("Contents to be updated must be of the same types");

            var updatedContent = new NATContent
            {
                type = type,
                Content = updatedContentDto.Content,
                simUUID = updatedContentDto.simUUID,
                Owner = user.Id
            };

            bool didUpdate = await contentRepo.UpdateContent(updatedContent, contentId, true);
            return didUpdate
                ? Ok("Content Updated!!")
                : StatusCode(500, new { message = "Content Update failed" });
        }

        [Authorize]
        [HttpDelete("{postId:int}")]
        public async Task<IActionResult> DeletePost([FromRoute] int postId)
        {
            var user = await postsRepo.GetLoggedInUser(User);
            if (user == null)
                return BadRequest("This user no longer exists...");

            bool deleted = await postsRepo.DeletePost(postId, user.Id);
            return deleted ? Ok(new { message = "Post Successfully Deleted" }) : StatusCode(400, new { message = "An error occurred" });
        }
    }
}
