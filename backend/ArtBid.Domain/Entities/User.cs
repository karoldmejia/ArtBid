namespace ArtBid.Domain.Entities
{

    public class User
{
    public Guid Id { get; private set; }
    public string? Username { get; private set; }
    public string? Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public decimal Balance { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Constructor principal para crear usuarios en código
    public User(string username, string email, string passwordHash, decimal initialBalance)
    {
        Id = Guid.NewGuid();
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Balance = initialBalance;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    // Constructor protegido para EF Core
    protected User() { }

    public bool VerifyPassword(string password)
    {
        return PasswordHash == HashPassword(password);
    }

    private string HashPassword(string password)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }

    public void Reserve(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be positive");

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient funds");

        Balance -= amount;
    }

    public void Release(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be positive");

        Balance += amount;
    }

    public void ConfirmCharge(decimal amount)
    {
        // Ya estaba reservado, no hacemos nada
    }
}
}

