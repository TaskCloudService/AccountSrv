using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentaion.Data;
using Presentation.Models;
using System.Security.Claims;

namespace AuthMicroservice.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _um;

    public ProfileController(ApplicationDbContext db, UserManager<ApplicationUser> um)
    {
        _db = db;
        _um = um;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _db.Profiles
            .Include(p => p.Addresses)
            .FirstOrDefaultAsync(p => p.UserId == userId);
        return Ok(profile);
    }

    [Authorize]
    [HttpGet("role-me")]
    public async Task<IActionResult> RoleCheck()
    {
        try
        {
            var user = await _um.GetUserAsync(User);
            var roles = await _um.GetRolesAsync(user);
            return Ok(new
            {
                user.Id,
                user.Email,
                Roles = roles
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving user roles.",
                error = ex.Message
            });
        }
    }

}