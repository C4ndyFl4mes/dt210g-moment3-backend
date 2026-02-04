using System.ComponentModel.DataAnnotations;

namespace App.DTOs;

public record UpdateThreadRequest
{
    [Required, MinLength(3), MaxLength(50)]
    public required string Title { get; set; }

    [Required, MinLength(3), MaxLength(400)]
    public required string InitialMessage { get; set; }
}