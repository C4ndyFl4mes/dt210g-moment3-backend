namespace App.DTOs;

public record PostResponse
{
    public int PostId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string AuthorUsername { get; set; }
    public required int AuthorId { get; set; }
}