namespace Frontend.Models;

public class DealerDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
public class CustomerDto { public Guid Id { get; set; } public string Name { get; set; } = ""; public Guid DealerId { get; set; } }
public class AccountCreatedResponse { public Guid Id { get; set; } public string Email { get; set; } = ""; public string Role { get; set; } = ""; public bool PasswordResetRequired { get; set; } }
