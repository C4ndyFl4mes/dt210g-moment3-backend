using System.ComponentModel.DataAnnotations;

namespace App.Models;

public class PostModel
{
    public int Id { get; set; }
    public required DateTime Published { get; set; }

    [Required, MinLength(3), MaxLength(400)]
    public required string Message { get; set; }
    

    // Post belongs to Thread.
    public required ThreadModel PostedOn { get; set; }

    // Posted by User.
    public required UserModel PostedBy { get; set; }
}