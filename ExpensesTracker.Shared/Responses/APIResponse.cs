namespace ExpensesTracker.Shared.Responses;

public class APIResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public DateTime ResponseDate { get; set; }
    
}
