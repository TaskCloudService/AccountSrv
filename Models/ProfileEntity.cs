using System.Net;
using System.Numerics;

namespace Presentation.Models
{
    public class ProfileEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public virtual ApplicationUser User { get; set; } = null!;
        public ICollection<AddressEntity> Addresses { get; set; } = new List<AddressEntity>();
    }
}
