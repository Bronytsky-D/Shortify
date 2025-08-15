namespace Shortify.Domain.Models
{
    public class LinkEntry : AuditableEntity<Guid>
    {
        public string OriginalUrl { get; set; }
        public string ShortenedUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
