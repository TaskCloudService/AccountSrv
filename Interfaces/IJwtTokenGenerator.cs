using Presentation.Models;

namespace Presentation.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> CreateTokenAsync(ApplicationUser user, IList<string> roles);
}
