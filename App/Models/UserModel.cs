using Microsoft.AspNetCore.Identity;

namespace App.Models;

public class UserModel : IdentityUser<int>
{
    public List<ThreadModel>? Threads { get; set; }
    public List<PostModel>? Posts { get; set; }
}