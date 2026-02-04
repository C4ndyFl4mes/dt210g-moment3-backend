namespace App.DTOs;

public record PostPostedResponse
{
    public int PostId { get; set; }
    public required DateTime Published { get; set; }
    public required string Message { get; set; }

    public required int ThreadId { get; set; }
    public required string Username { get; set; }
}