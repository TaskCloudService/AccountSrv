using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using Presentation.Security;

[ApiController]
[Route("internal/accounts")]                
public sealed class InternalAccountsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    public InternalAccountsController(UserManager<ApplicationUser> users) => _users = users;

    [HttpDelete("{userId:guid}")]
    [ApiKey]                              
    public async Task<IActionResult> HardDelete(Guid userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null)
            return NotFound();

        var result = await _users.DeleteAsync(user);
        return result.Succeeded ? NoContent() :
               BadRequest(result.Errors.Select(e => e.Description));
    }
}
