namespace App.DTOs;

public record AllPostsResponse
{
    public required List<PostResponse> Posts { get; set; }
}