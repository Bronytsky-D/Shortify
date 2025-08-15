namespace Shortify.Application.DTOs
{
    public class RegisterAuthDTO
    {
        public string Email { get; set; } = "Di@example.com";
        public string Password { get; set; } = "P@ssw0rd";
        public string UserName { get; set; } = "Di";
        public string role { get; set; } = "User";
    }
}
