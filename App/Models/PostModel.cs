namespace App.Models;

public class PostModel
{
    public int Id { get; set; }
    public int? ParentPostId { get; set; }
    public required DateTime Published { get; set; }
    public required string Message { get; set; }
    

    // Post belongs to Thread.
    public required ThreadModel PostedOn { get; set; }

    // Posted by User.
    public required UserModel PostedBy { get; set; }

    // Replies to Post.
    public List<PostModel>? Replies { get; set; }
}