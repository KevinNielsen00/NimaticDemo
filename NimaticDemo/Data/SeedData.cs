using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        var dealer = await db.Dealers.FirstOrDefaultAsync(d => d.Name == "Default Dealer");

        if (dealer == null)
        {
            dealer = new Dealer
            {
                Name = "Default Dealer"
            };

            db.Dealers.Add(dealer);
            await db.SaveChangesAsync();
        }

        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Name == "Default Customer");

        if (customer == null)
        {
            customer = new Customer
            {
                Name = "Default Customer",
                DealerId = dealer.Id
            };

            db.Customers.Add(customer);
            await db.SaveChangesAsync();
        }

        var adminEmail = "admin@nimatic.com";

        var admin = await db.Accounts.FirstOrDefaultAsync(a => a.Email == adminEmail);

        if (admin == null)
        {
            admin = new Account
            {
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = AccountRole.Admin
            };

            db.Accounts.Add(admin);
        }

        var dealerEmail = "dealer@nimatic.com";

        var dealerAccount = await db.Accounts.FirstOrDefaultAsync(a => a.Email == dealerEmail);

        if (dealerAccount == null)
        {
            dealerAccount = new Account
            {
                Name = "Dealer User",
                Email = dealerEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dealer123!"),
                Role = AccountRole.Dealer,
                DealerId = dealer.Id
            };

            db.Accounts.Add(dealerAccount);
        }

        var customerEmail = "customer@nimatic.com";

        var customerAccount = await db.Accounts.FirstOrDefaultAsync(a => a.Email == customerEmail);

        if (customerAccount == null)
        {
            customerAccount = new Account
            {
                Name = "Customer User",
                Email = customerEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                Role = AccountRole.Customer,
                CustomerId = customer.Id
            };

            db.Accounts.Add(customerAccount);
        }

        await db.SaveChangesAsync();
    }
}