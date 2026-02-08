using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Models;
using App.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PostsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/posts
    [HttpGet]
    public async Task<ActionResult<List<PostResponse>>> GetAllPosts()
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .Select(post => new PostResponse
            {
                PostId = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                AuthorUsername = post.Author.UserName!,
                AuthorId = post.Author.Id
            })
            .ToListAsync();

        return Ok(posts);
    }

    // GET: api/posts/:id
    [HttpGet("{id}")]
    public async Task<ActionResult<PostResponse>> GetPost(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound(new { message = "Post not found." });
        }

        return Ok(new PostResponse
        {
            PostId = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorUsername = post.Author.UserName!,
            AuthorId = post.Author.Id
        });
    }

    // POST: api/posts
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostResponse>> CreatePost(PostRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        var author = await _context.Users.FindAsync(userId);
        if (author == null)
        {
            return BadRequest(new { message = "User not found." });
        }

        var post = new PostModel
        {
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.Now,
            Author = author,
            AuthorId = userId
        };

        _context.Posts.Add(post);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { message = "An error occurred while saving the post.", error = ex.Message });
        }

        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, new PostResponse
        {
            PostId = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorUsername = post.Author.UserName!,
            AuthorId = post.Author.Id
        });
    }

    // PUT: api/posts/:id
    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<PostResponse>> UpdatePost(int id, PostRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        var post = await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound(new { message = "Post not found." });
        }

        if (post.AuthorId != userId)
        {
            return Forbid();
        }

        post.Title = request.Title;
        post.Content = request.Content;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { message = "An error occurred while updating the post.", error = ex.Message });
        }

        return Ok(new PostResponse
        {
            PostId = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorUsername = post.Author.UserName!,
            AuthorId = post.Author.Id
        });
    }

    // DELETE: api/posts/:id
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePost(int id)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound(new { message = "Post not found." });
        }

        if (post.AuthorId != userId)
        {
            return Forbid();
        }

        _context.Posts.Remove(post);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { message = "An error occurred while deleting the post.", error = ex.Message });
        }

        return NoContent();
    }
}

