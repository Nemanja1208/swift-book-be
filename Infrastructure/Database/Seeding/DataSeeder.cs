using System.Security.Cryptography;
using System.Text;
using Bogus;
using Domain.Models.Accounts;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Seeding
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            // 1️⃣ Create roles
            var roles = new[]
            {
            new Role { Name = "Admin" },
            new Role { Name = "User" },
            new Role { Name = "CompanyUser" },
            new Role { Name = "Auditor" },
            new Role { Name = "Manager" }
        };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();

            var adminRole = roles.First(r => r.Name == "Admin");
            var userRole = roles.First(r => r.Name == "User");
            var companyRole = roles.First(r => r.Name == "CompanyUser");

            var faker = new Faker("en");

            var users = new List<User>();

            // 2️⃣ Seed admin user
            var admin = CreateUserWithPassword("admin", "admin@bank.com", "admin123", new[] { adminRole });
            users.Add(admin);

            // 3️⃣ Seed regular users
            for (int i = 0; i < 5; i++)
            {
                var username = faker.Internet.UserName();
                var email = faker.Internet.Email();
                var password = "user123";

                var user = CreateUserWithPassword(username, email, password, new[] { userRole });

                users.Add(user);
            }

            // 4️⃣ Seed company/internal users
            for (int i = 0; i < 3; i++)
            {
                var username = $"employee_{faker.Random.Number(100, 999)}";
                var email = faker.Internet.Email();

                var user = CreateUserWithPassword(username, email, "company123", new[] { companyRole });
                users.Add(user);
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // 5️⃣ Create bank accounts for each user
            var accounts = new List<BankAccount>();

            foreach (var user in users)
            {
                var count = faker.Random.Int(1, 2);
                for (int i = 0; i < count; i++)
                {
                    accounts.Add(new BankAccount
                    {
                        Id = Guid.NewGuid(),
                        AccountNumber = $"ACC-{faker.Random.Number(100000, 999999)}",
                        OwnerName = user.Username,
                        Currency = faker.Finance.Currency().Code,
                        Balance = faker.Finance.Amount(100, 10000),
                        OpenedAt = faker.Date.Past(3),
                        IsActive = true,
                        UserId = user.Id
                    });
                }
            }

            await context.BankAccounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();
        }

        private static User CreateUserWithPassword(string username, string email, string password, Role[] roles)
        {
            using var hmac = new HMACSHA512();
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            var salt = hmac.Key;

            return new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Roles = roles.Select(role => new UserRole
                {
                    RoleId = role.Id
                }).ToList()
            };
        }
    }
}
