using System.ComponentModel.DataAnnotations;

namespace App.DTOs;

public record LoginRequest
{
    [Required, MinLength(3), MaxLength(28)]
    public required string Username { get; set; }

    public required string Password { get; set; }
}