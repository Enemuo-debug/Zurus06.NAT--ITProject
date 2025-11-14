namespace server.dtos;

public class HTTPResponseStructure
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }

    public HTTPResponseStructure(bool success, string message, object data = null)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}
