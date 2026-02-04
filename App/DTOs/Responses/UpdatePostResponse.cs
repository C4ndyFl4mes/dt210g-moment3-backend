namespace App.DTOs;

public record UpdatePostResponse
{
    public required int PostId { get; set; }
    public required string Message { get; set; }
}