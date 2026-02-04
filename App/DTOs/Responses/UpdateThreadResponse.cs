namespace App.DTOs;

public record UpdateThreadResponse
{
    public required int ThreadId { get; set; }
    public required string Title { get; set; }
    public required string InitialMessage { get; set; }
}