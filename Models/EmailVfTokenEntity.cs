namespace Presentation.Models
{
    public class EmailVfTokenEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Code { get; set; } = default!; 
        public DateTime ExpiresAtUtc { get; set; }
        public bool Used { get; set; }

        public virtual ApplicationUser User { get; set; } = default!;
    }
}
