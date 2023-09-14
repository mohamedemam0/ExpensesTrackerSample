using ExpensesTracker.Shared.Enums;

namespace ExpensesTracker.Shared.DTOs;

public class WalletDto
{
    public string? Id { get; set; }
    public WalletType Type { get; set; }

    public string? TypeName { get; set; }

    public string? BankName { get; set; }

    public string? Name { get; set; }

    public string? Iban { get; set; }

    public string? AccountType { get; set; }

    public string? Swift { get; set; }

    public decimal Balance { get; set; }

    public string? Currency { get; set; }

    public string? Username { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }
}
