namespace server.NATModels;

public class NATRequests
{
    public int Id { get; set; }
    public required string EmailAddress { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}
