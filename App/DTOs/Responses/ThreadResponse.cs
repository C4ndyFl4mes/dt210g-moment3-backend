namespace App.DTOs;

public record ThreadResponse
{
    public int ThreadId { get; set; }
    public required string Title { get; set; }
    public required DateTime Published { get; set; }
    public required string InitialMessage { get; set; }
    public required string CreatedBy { get; set; }
}