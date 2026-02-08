using System.ComponentModel.DataAnnotations;

namespace App.DTOs;

public record PostRequest
{
    [Required, MinLength(3), MaxLength(200)]
    public required string Title { get; set; }

    [Required, MinLength(3)]
    public required string Content { get; set; }
}