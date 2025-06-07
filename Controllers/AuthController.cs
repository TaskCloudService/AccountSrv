using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.DTOs;
using Presentation.Interfaces;
using Presentation.Models;
using Presentation.Services;
using static Presentation.DTOs.AuthController;

namespace AuthMicroservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGen;
    private readonly IVerificationService _verifier;
    private readonly RoleManager<ApplicationRole> _rm;
    private readonly IRefreshTokenService _refreshSvc;
    private readonly IWebHostEnvironment _env;
    private readonly bool _secureCookie;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGen,
        IVerificationService verifier,
        RoleManager<ApplicationRole> roleManager,
        IRefreshTokenService refreshToken,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _tokenGen = tokenGen;
        _verifier = verifier;
        _rm = roleManager;
        _refreshSvc = refreshToken;
        _env = env;
        _secureCookie = env.IsProduction();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(new
                {
                    success = false,
                    errors = createResult.Errors.Select(e => e.Description)
                });

            if (!await _rm.RoleExistsAsync("User"))
                await _rm.CreateAsync(new ApplicationRole { Name = "User" });

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
                return StatusCode(500, new
                {
                    success = false,
                    errors = roleResult.Errors.Select(e => e.Description)
                });

            await _verifier.SendCodeAsync(user);

            return Ok(new
            {
                success = true,
                message = "Registration successful. A verification code has been sent to your email.",
                userId = user.Id,
                requiresVerification = true
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected error occurred during registration.",
                detail = ex.Message
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { success = false, message = "Invalid credentials." });

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                await _verifier.SendCodeAsync(user);   
                return Ok(new
                {
                    success = true,      
                    requiresVerification = true,
                    userId = user.Id,
                    message = "A verification code has been sent to your email."
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenGen.CreateTokenAsync(user, roles);
            var refreshToken = await _refreshSvc.GenerateAsync(user.Id); 

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = _secureCookie,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            return Ok(new
            {
                success = true,
                requiresVerification = false,
                token
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected error occurred during login.",
                detail = ex.Message
            });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var old = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(old)) return Unauthorized();

        var userId = await _refreshSvc.ValidateAsync(old);
        if (userId == null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var newJwt = await _tokenGen.CreateTokenAsync(user, roles);
        var newRefresh = await _refreshSvc.RotateAsync(old, userId.Value);

        Response.Cookies.Append("refreshToken", newRefresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = _secureCookie,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return Ok(new { token = newJwt });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> Verify(VerifyDto dto)
    {
        try
        {
            var valid = await _verifier.VerifyCodeAsync(dto.UserId, dto.Code);
            if (!valid)
                return BadRequest(new { success = false, message = "Invalid or expired verification code." });

            var user = await _userManager.FindByIdAsync(dto.UserId.ToString())!;
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            var refreshToken = await _refreshSvc.GenerateAsync(user.Id);

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = _secureCookie,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            return Ok(new { success = true, message = "Email successfully verified." });

        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected error occurred while verifying your email.",
                detail = ex.Message
            });
        }
    }

    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromBody] SendCodeDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return NotFound(new { success = false, message = "User not found." });

        await _verifier.SendCodeAsync(user);
        return Ok(new { success = true, message = "Verification code sent." });
    }


    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("refreshToken");

        return Ok(new { success = true, message = "Logged out successfully." });
    }


    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new
                {
                    success = false,
                    errors = result.Errors.Select(e => e.Description)
                });

            return Ok(new
            {
                success = true,
                message = "User deleted successfully."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected error occurred while deleting the user.",
                detail = ex.Message
            });
        }
    }
}
