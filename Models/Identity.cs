using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Presentation.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public virtual ProfileEntity? Profile { get; set; }
        public virtual ICollection<RefreshTokenEntity> RefreshTokens { get; set; }
        = new List<RefreshTokenEntity>();
    }

    public class ApplicationRole : IdentityRole<Guid> { }
}
