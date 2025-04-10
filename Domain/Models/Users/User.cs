using Domain.Models.Accounts;

namespace Domain.Models.Users
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = [];
        public byte[] PasswordSalt { get; set; } = [];

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

        public List<UserRole> Roles { get; set; } = new();
        public List<BankAccount> BankAccounts { get; set; } = new();
    }
}
