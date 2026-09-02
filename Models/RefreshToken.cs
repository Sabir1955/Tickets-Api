namespace Authentication.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = "";

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }

        public User User { get; set; } = null!;

    }
}
