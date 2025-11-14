using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using server.dtos;
using server.NATModels;
using server.tools;
using server.MappersAndExtensions;
using server.Interfaces;

namespace server.controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly EmailService emailService;
        private readonly EmailCodeVerification ecv;
        private readonly SignInManager<NATUser> signInManager;
        private readonly UserManager<NATUser> userManager;
        private readonly IConfiguration config;
        private readonly JWTToken jWtToken;
        private readonly IPosts postsRepo;
        public AccountController(EmailService _emailService, EmailCodeVerification _ecv, SignInManager<NATUser> _signInManager, UserManager<NATUser> _userManager, IConfiguration _config, JWTToken _token, IPosts _postsRepo)
        {
            emailService = _emailService;
            ecv = _ecv;
            signInManager = _signInManager;
            userManager = _userManager;
            config = _config;
            jWtToken = _token;
            postsRepo = _postsRepo;
        }

        [HttpPost("email-verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Create email Subject
            string Subject = "🔐 Your Zurus06.NAT Login Code";
            string? code = await ecv.CreateCode(verifyEmailDto.EmailAddress);
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new HTTPResponseStructure(false, "This email address has been linked to an account already"));
            }
            string Body = ecv.GenerateBody(code);
            await emailService.SendEmailAsync(verifyEmailDto.EmailAddress, Subject, Body);
            HTTPResponseStructure response = new HTTPResponseStructure(true, "Email verification code sent successfully");
            return Ok(response);
        }
        [HttpPost("create/{id}")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto createAccountDto, [FromRoute] string id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var newUser = new NATUser
            {
                UserName = createAccountDto.UserName,
                Email = createAccountDto.EmailAddress,
                Bio = createAccountDto.Bio,
                DisplayName = createAccountDto.DisplayName,
                niche = (Niche) createAccountDto.Niche
            };
            // Verify the code
            if (!await ecv.VerifyCode(createAccountDto.EmailAddress, id))
            {
                HTTPResponseStructure responseStructure = new HTTPResponseStructure(false, "The verification code is invalid or has expired");
                return BadRequest(responseStructure);
            }
            var createdUser = await userManager.CreateAsync(newUser, createAccountDto.Password);
            HTTPResponseStructure response;
            if (createdUser.Succeeded)
            {
                response = new HTTPResponseStructure(true, "Account created successfully");
                return Ok(response);
            }
            else
            {
                response = new HTTPResponseStructure(false, "Account creation failed", createdUser.Errors);
                return BadRequest(response);
            }
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginForm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await userManager.FindByEmailAsync(loginForm.EmailAddress);
            if (user == null)
            {
                return BadRequest("User no longer exists or has been deleted");
            }
            var result = await signInManager.CheckPasswordSignInAsync(user, loginForm.Password, false);
            if (result.Succeeded)
            {
                // Generate JWT token
                string token = jWtToken.CreateToken(user);

                // Create a cookie with the JWT
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.Now.AddDays(1)
                };

                Response.Cookies.Append("NAT-Authentication", token, cookieOptions);
                HTTPResponseStructure response = new HTTPResponseStructure(true, "Login successful");
                return Ok(response);
            }
            return Unauthorized("Incorrect username or password");
        }

        // Get the User details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetails([FromRoute] string id)
        {
            var user = await userManager.FindByIdAsync(id);
            HTTPResponseStructure response;
            if (user == null)
            {
                response = new HTTPResponseStructure(false, "User not found");
                return NotFound(response);
            }
            response = new HTTPResponseStructure(true, "User Details fetched Successfully", user.UserDetails());
            return Ok(response);
        }

        // Get the current logged in user details
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserDetails()
        {
            var email = User.GetUserEmail();
            HTTPResponseStructure response;
            if (string.IsNullOrEmpty(email))
            {
                response = new HTTPResponseStructure(false, "Unauthorized access");
                return Unauthorized(response);
            }
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                response = new HTTPResponseStructure(false, "User not found");
                return NotFound(response);
            }
            List<OutputPostDto> userPosts = await postsRepo.GetAllUsersPosts(user.Id);
            UserDetailsDto userDetailsDto = JointMapper.MapToUserDetailsDto(user.UserDetails(), userPosts);
            response = new HTTPResponseStructure(true, "Current User Details fetched Successfully", userDetailsDto);
            return Ok(response);
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            if (Request.Cookies.ContainsKey("NAT-Authentication"))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.Now.AddDays(-1) // Set the expiration date to the past
                };
                Response.Cookies.Append("NAT-Authentication", "", cookieOptions);
            }
            return Ok("You have been logged out successfully");
        }
    }
}