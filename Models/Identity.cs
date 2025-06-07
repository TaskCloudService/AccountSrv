using Microsoft.AspNetCore.Identity;


namespace Presentation.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public virtual ICollection<RefreshTokenEntity> RefreshTokens { get; set; }
        = new List<RefreshTokenEntity>();
    }

    public class ApplicationRole : IdentityRole<Guid> { }
}
