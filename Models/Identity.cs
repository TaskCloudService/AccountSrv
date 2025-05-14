using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Presentation.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public virtual ProfileEntity? Profile { get; set; }
    }

    public class ApplicationRole : IdentityRole<Guid> { }
}
