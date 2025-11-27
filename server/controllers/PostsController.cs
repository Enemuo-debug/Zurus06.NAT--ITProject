using server.Interfaces;
using server.NATModels;
using server.dtos;
using Microsoft.AspNetCore.Mvc;
using server.MappersAndExtensions;
using Microsoft.AspNetCore.Authorization;
using server.tools;
using Microsoft.AspNetCore.Identity;

namespace server.controllers
{
    [ApiController]
    [Route("posts")]
    public class PostsController : ControllerBase
    {
        private readonly IPosts postsRepo;
        private readonly IContent contentRepo;
        private readonly ISimulation simRepo;
        private readonly FileUpload cloudinaryService;
        private readonly UserManager<NATUser> userManager;

        public PostsController(IPosts _postsRepo, IContent _contentRepo, FileUpload _cloudinaryService, UserManager<NATUser> _userManager, ISimulation _simRepo)
        {
            postsRepo = _postsRepo;
            contentRepo = _contentRepo;
            cloudinaryService = _cloudinaryService;
            userManager = _userManager;
            simRepo = _simRepo;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await postsRepo.GetAllPosts();
            HTTPResponseStructure response;
            var detailedPosts = new List<OutputPostDto>();
            foreach (var post in posts)
            {
                detailedPosts.Add(await post.PostDetails(contentRepo, simRepo));
            }
            response = new(true, "Post Loaded Successfully", detailedPosts);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("new")]
        public async Task<IActionResult> CreateANewPost([FromBody] NewPostDto newPost)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            HTTPResponseStructure response;
            var email = User.GetUserEmail() ?? "";
            var post = await postsRepo.CreateANewPost(newPost.EncryptPostDto(), email);
            if (post == null)
            {
                response = new(false, "Post Creation Failed");
                return BadRequest(response);
            }
            response = new(true, "Post Created Successfully", await post.PostDetails(contentRepo, simRepo));
            return Ok(response);
        }

        [Authorize]
        [HttpPost("new-content")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePostContent([FromForm] NewContentDto newContent)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            HTTPResponseStructure response;
            var email = User.GetUserEmail() ?? "";
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                response = new(false, "User not found");
                return Unauthorized(response);
            }
            var file = newContent.File;
            string url = string.Empty;
            if (file != null && file.Length != 0)
            {
                url = await cloudinaryService.UploadImageAsync(file);
                if (string.IsNullOrEmpty(url))
                {
                    response = new(false, "File upload failed");
                    return BadRequest(response);
                }
            }
            var cipherContent = await contentRepo.CreateNewContent(newContent, user.Id, url);
            if (cipherContent == null) return BadRequest("Could not create content");
            var plainContent = await cipherContent.DecryptContentDto(simRepo);
            return Ok(plainContent);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPostById([FromRoute] int id)
        {
            var post = await postsRepo.GetPostById(id);
            if (post == null) return NotFound("Post not found");
            return Ok(await post.PostDetails(contentRepo, simRepo));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsByUserId([FromRoute] string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest("This user no longer exists...");
            var posts = await postsRepo.GetAllUsersPosts(userId);
            return Ok(posts);
        }

        [Authorize]
        [HttpPut("edit/{postId:int}")]
        public async Task<IActionResult> EditPost([FromRoute] int postId, [FromBody] UpdatePostDto editPost)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Get the logged in user
            var user = await postsRepo.GetLoggedInUser(User);
            if (user == null) return BadRequest("This user no longer exists...");
            editPost = editPost.EncryptPostDto();
            string email = User.GetUserEmail() ?? string.Empty;
            bool isUpdated = await postsRepo.UpdatePost(editPost, postId, email);
            return isUpdated ? Ok("Post Successfully Updated!!!") : StatusCode(403, new { message = "The post is being modified by someone else at this time" });
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

        [Authorize]
        [HttpPost("comment/{postId}")]
        public async Task<IActionResult> CreateComment([FromRoute] int postId, [FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return BadRequest(new { message = "Comment cannot be empty" });

            var user = await postsRepo.GetLoggedInUser(User);
            if (user == null) return Unauthorized(new { message = "User not found" });

            var post = await postsRepo.GetPostById(postId);
            if (post == null) return NotFound(new { message = "Post not found" });

            var comment = await postsRepo.CreateComment(postId, message, user.Id);
            if (comment == null) return BadRequest(new { message = "Could not create comment" });

            return Ok(new { message = "Comment created successfully"});
        }

        [HttpGet("{postId}/comments")] 
        public async Task<IActionResult> GetCommentsByPostId([FromRoute] int postId)
        {
            var post = await postsRepo.GetPostById(postId);
            if (post == null) return NotFound(new { message = "Post not found" });

            var comments = await postsRepo.GetCommentsForPost(postId);
            // Build a DTO with display name
            var result = new List<CommentOutputDto>();
            foreach (var c in comments)
            {
                var commenter = await userManager.FindByIdAsync(c.UserId);
                string niche = commenter!.niche.ToString();
                string displayName = commenter!.DisplayName;
                result.Add(new CommentOutputDto { Id = c.Id, UserId = c.UserId, DisplayName = displayName, Message = c.Message, Niche = niche});
            }

            return Ok(result);
        }
    }
}
