using System.ComponentModel.DataAnnotations;

namespace App.DTOs;

public record PaginateRequest
{
    [Required]
    public required int perPage { get; set; }
    
    [Required]
    public required int currentPage { get; set; }
}