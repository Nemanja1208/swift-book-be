namespace Domain.Models.Users
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<UserRole> Users { get; set; } = new();
    }
}
