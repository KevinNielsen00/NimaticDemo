namespace Backend.Models
{
    public class Dealer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public List<Account> Accounts { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();
    }
}
