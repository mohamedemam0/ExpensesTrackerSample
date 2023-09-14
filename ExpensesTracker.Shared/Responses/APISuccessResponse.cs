

namespace ExpensesTracker.Shared.Responses;

public class APISuccessResponse<T>: APIResponse
{
    public APISuccessResponse()
    {
        IsSuccess = true;
        ResponseDate = DateTime.UtcNow;
    }

    public APISuccessResponse(T? record) : this()
    {
        Record = record;
    }

    public APISuccessResponse(string message, T record) : this()
    {
        Message = message;
        Record = record;
    }

    public APISuccessResponse(string message) : this()
    {
        Message = message;
    }

    public T? Record { get; set; }
}
