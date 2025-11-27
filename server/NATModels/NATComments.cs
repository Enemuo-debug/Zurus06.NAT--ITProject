namespace server.NATModels;

public class NATComments
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required int PostId { get; set; }
    public required string Message { get; set; }
}
