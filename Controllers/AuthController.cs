using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using Presentation.Services;

namespace AuthMicroservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGen;
    private readonly IVerificationService _verifier;
    private readonly RoleManager<ApplicationRole> _rm;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGen,
        IVerificationService verifier,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _tokenGen = tokenGen;
        _verifier = verifier;
        _rm = roleManager;
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
            if (user == null)
                return Unauthorized(new { success = false, message = "Invalid credentials." });

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { success = false, message = "Invalid credentials." });

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest(new { success = false, message = "Email address has not been verified." });

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenGen.CreateTokenAsync(user, roles);

            return Ok(new { success = true, token });
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

    public record RegisterDto(string Email, string Password, string FirstName, string LastName);
    public record LoginDto(string Email, string Password);
    public record VerifyDto(Guid UserId, string Code);
}
