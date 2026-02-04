using System.ComponentModel.DataAnnotations;

namespace App.DTOs;

public record UpdatePostRequest
{
    [Required, MinLength(3), MaxLength(400)]
    public required string Message { get; set; }
}