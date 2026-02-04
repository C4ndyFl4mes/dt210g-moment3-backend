using App.Models;

namespace App.DTOs;

public record ThreadPostsPaginatedResponse
{
    public required Pagination Pagination { get; set; }
    public required List<PostResponse> Posts { get; set; }
}