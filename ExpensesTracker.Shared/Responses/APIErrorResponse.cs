

namespace ExpensesTracker.Shared.Responses;

public class APIErrorResponse : APIResponse
{
    public APIErrorResponse()
    {
        IsSuccess = false;
        ResponseDate = DateTime.UtcNow;
    }
    public APIErrorResponse(string message) : this()
    {
        Message = message;
    }

    public APIErrorResponse(string message, IEnumerable<string>? errors) : this()
    {
        Message = message;
        Errors = errors;
    }
    
    public IEnumerable<string>? Errors { get; set; }
}
