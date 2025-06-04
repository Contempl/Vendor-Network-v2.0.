namespace Product.Domain.Result;

public class Response
{
    public bool IsSuccess => ErrorMessage == null;

    public string ErrorMessage { get; set; }

    public int? ErrorCode { get; set; }
}

public class Response<T> : Response
{
    public T Data { get; set; }
    
    public Response(string errorMessage, int errorCode, T data)
    {
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        Data = data;
    }
    
    public Response() { }
}