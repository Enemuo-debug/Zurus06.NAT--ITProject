using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using server.dtos;
using server.NATModels;
using server.tools;

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
        public AccountController(EmailService _emailService, EmailCodeVerification _ecv, SignInManager<NATUser> _signInManager, UserManager<NATUser> _userManager, IConfiguration _config, JWTToken _token)
        {
            emailService = _emailService;
            ecv = _ecv;
            signInManager = _signInManager;
            userManager = _userManager;
            config = _config;
            jWtToken = _token;
        }

        [HttpPost("email-verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Create email Subject
            string Subject = $"WELCOME TO Zurus06.NAT";
            string Body = $"Verify your email address using this link {config["Jwt:URL"]}{ecv.CreateCode(verifyEmailDto.EmailAddress).Substring(0, int.Parse(config["Secrets:codeLength"] ?? "10"))}";
            await emailService.SendEmailAsync(verifyEmailDto.EmailAddress, Subject, Body);
            return Ok($"Email verification code has been sent to {verifyEmailDto.EmailAddress}");
        }
        [HttpPost("create/{id}")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto createAccountDto, [FromRoute] string id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Verify the code
            if (!ecv.VerifyCode(createAccountDto.EmailAddress, id)) return Unauthorized("Incorrect code");
            var newUser = new NATUser
            {
                UserName = createAccountDto.UserName,
                Email = createAccountDto.EmailAddress,
                Bio = createAccountDto.Bio,
                DisplayName = createAccountDto.DisplayName,
                niche = NicheFunctions.MapNiche(createAccountDto.Niche)
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
                string token = jWtToken.CreateToken(user);
                return Ok(new { message = "Login Successful", token });
            }
            return Unauthorized("Incorrect username or password");
        }
    }
}