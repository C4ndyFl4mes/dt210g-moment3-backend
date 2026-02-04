namespace App.DTOs;

public record AllThreadsResponse
{
    public required List<ThreadResponse> Threads { get; set; }
}