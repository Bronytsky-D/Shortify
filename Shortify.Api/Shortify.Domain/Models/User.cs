using Microsoft.AspNetCore.Identity;

namespace Shortify.Domain.Models
{
    public class User : IdentityUser<string>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();

        public User()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

}
