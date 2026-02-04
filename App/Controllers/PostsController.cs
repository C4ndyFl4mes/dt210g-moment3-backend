using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Models;
using App.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Client;

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

    // GET: api/posts/thread/:id
    [HttpGet("thread/{id}")]
    public async Task<ActionResult<ThreadPostsPaginatedResponse>> GetPostsFromThread(PaginateRequest request, int id)
    {

        int totalPosts = await _context.Posts.Where(post => post.PostedOn.Id == id).CountAsync();

        if (totalPosts == 0)
        {
            return NotFound(new { message = "No posts found." });
        }

        int totalPages = (int)Math.Ceiling((double)totalPosts / request.perPage);

        if (request.currentPage < 1 || request.currentPage > totalPages)
        {
            return BadRequest(new { message = "Invalid page number." });
        }

        List<PostResponse> pagedPosts = await _context.Posts
            .Include(p => p.PostedBy)
            .Where(post => post.PostedOn.Id == id)
            .OrderByDescending(post => post.Published)
            .Skip((request.currentPage - 1) * request.perPage)
            .Take(request.perPage)
            .Select(post => new PostResponse
            {
                PostId = post.Id,
                Published = post.Published,
                Message = post.Message,
                Username = post.PostedBy.UserName!
            })
            .ToListAsync();

        Items items = new Items
        {
            PerPage = request.perPage,
            Count = pagedPosts.Count,
            Total = totalPosts
        };

        Pagination pagination = new Pagination
        {
            LastPage = totalPages,
            CurrentPage = request.currentPage,
            Items = items
        };

        return Ok(new ThreadPostsPaginatedResponse
        {
            Pagination = pagination,
            Posts = pagedPosts
        });
    }

    // POST: api/posts/thread/:id
    [Authorize]
    [HttpPost("thread/{id}")]
    public async Task<ActionResult<PostPostedResponse>> PostAPost(PostRequest request, int id)
    {
        ThreadModel? postedOn = await _context.Threads.Where(thread => thread.Id == id).FirstOrDefaultAsync();

        if (postedOn == null)
        {
            return BadRequest(new { message = "Cannot publish post on a thread that does not exist." });
        }

        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        UserModel? postedBy = await _context.Users.FindAsync(userId);

        if (postedBy == null)
        {
            return BadRequest(new { message = "A user that does not exist cannot publish a post." });
        }

        PostModel post = new PostModel
        {
            Message = request.Message,
            PostedBy = postedBy,
            PostedOn = postedOn,
            Published = DateTime.UtcNow
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }

        return Ok(new PostPostedResponse
        {
            PostId = post.Id,
            Published = post.Published,
            Message = post.Message,
            ThreadId = post.PostedOn.Id,
            Username = post.PostedBy.UserName!
        });
    }

    // PUT: api/posts/post/:id
    [Authorize]
    [HttpPut("post/{id}")]
    public async Task<ActionResult<UpdatePostResponse>> UpdatePost(UpdatePostRequest request, int id)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        PostModel? oldPost = await _context.Posts.Include(p => p.PostedBy).Where(post => post.Id == id).FirstOrDefaultAsync();

        if (oldPost == null)
        {
            return NotFound(new { message = "Cannot edit post that does not exist." });
        }

        if (oldPost.PostedBy.Id != userId)
        {
            return Unauthorized(new { message = "You cannot edit a post that belongs to another user." });
        }

        oldPost.Message = request.Message;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostModelExists(id))
            {
                return NotFound(new { message = "Cannot edit post that does not exist." });
            }
            return StatusCode(500, new { message = "A concurrency error occurred while updating the post." });
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { message = "Database error while updating post.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }

        return Ok(new UpdatePostResponse
        {
            PostId = oldPost.Id,
            Message = oldPost.Message
        });
    }

    // DELETE: api/posts/post/:id
    [Authorize]
    [HttpDelete("post/{id}")]
    public async Task<ActionResult<PostDeletedResponse>> DeletePost(int id)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        PostModel? post = await _context.Posts.Include(p => p.PostedBy).Where(post => post.Id == id).FirstOrDefaultAsync();

        if (post == null)
        {
            return NotFound(new { message = "Cannot delete a post that does not exist." });
        }

        if (post.PostedBy.Id != userId)
        {
            return Unauthorized(new { message = "You cannot delete a post that belongs to another user." });
        }


        _context.Posts.Remove(post);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { message = "Database error while deleting a post.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }

        return Ok(new PostDeletedResponse
        {
            PostId = post.Id
        });
    }

    private bool PostModelExists(int id)
    {
        return _context.Posts.Any(e => e.Id == id);
    }
}

