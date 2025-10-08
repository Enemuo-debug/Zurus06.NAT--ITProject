using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            string Subject = $"WELCOME TO Zurus06.NAT";
            string? code = await ecv.CreateCode(verifyEmailDto.EmailAddress);
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("The email you have provided is already exists on an account");
            }
            string Body = $"Verify your email address using this link {config["Jwt:URL"]}{code}";
            await emailService.SendEmailAsync(verifyEmailDto.EmailAddress, Subject, Body);
            return Ok($"Email verification code has been sent to {verifyEmailDto.EmailAddress}");
        }
        [HttpPost("create/{id}")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto createAccountDto, [FromRoute] string id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Verify the code
            if (!await ecv.VerifyCode(createAccountDto.EmailAddress, id)) return Unauthorized("Incorrect code");
            var newUser = new NATUser
            {
                UserName = createAccountDto.UserName,
                Email = createAccountDto.EmailAddress,
                Bio = createAccountDto.Bio,
                DisplayName = createAccountDto.DisplayName,
                niche = (Niche) createAccountDto.Niche
            };
            var createdUser = await userManager.CreateAsync(newUser, createAccountDto.Password);
            if (createdUser.Succeeded) return Ok("New User has been created successfully");
            return BadRequest(createdUser.Errors);
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
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddDays(1)
                };

                Response.Cookies.Append("NAT-Authentication", token, cookieOptions);
                return Ok(new { message = "Login Successful" });
            }
            return Unauthorized("Incorrect username or password");
        }

        // Get the User details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetails([FromRoute] string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound("User not found");
            return Ok(user.UserDetails());
        }

        // Get the current logged in user details
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserDetails()
        {
            var email = User.GetUserEmail();
            if (string.IsNullOrEmpty(email)) return Unauthorized("You are not logged in");
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized("User not found");
            List<OutputPostDto> userPosts = await postsRepo.GetAllUsersPosts(user.Id);
            UserDetailsDto userDetailsDto = JointMapper.MapToUserDetailsDto(user.UserDetails(), userPosts);
            return Ok(userDetailsDto);
        }
    }
}