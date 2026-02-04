namespace App.DTOs;

public record PostResponse
{
    public int PostId { get; set; }
    public required DateTime Published { get; set; }
    public required string Message { get; set; }
    public required string Username { get; set; }
}