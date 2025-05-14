using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Presentation.Models
{
    public class AddressEntity
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }

        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public virtual ProfileEntity Profile { get; set; } = null!;
    }
}
