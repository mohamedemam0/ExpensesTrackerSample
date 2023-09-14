
using ExpensesTracker.Shared.Enums;

namespace ExpensesTracker.Shared.DTOs;

public class WalletSummaryDto
{
    public string? Id { get; set; }
    public WalletType Type{ get; set; }
    public decimal Balance { get; set; }
    public string? Name { get; set; }
    public string? Currency { get; set; }
}