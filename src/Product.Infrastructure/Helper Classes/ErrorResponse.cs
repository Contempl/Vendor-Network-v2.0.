namespace Product.Infrastructure.Helper_Classes;

public class ErrorResponse
{
	private readonly string _message;
	private readonly string _details;

    public ErrorResponse(string message, string details)
    {
        _message = message;
        _details = details;
    }
}
