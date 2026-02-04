using App.Data;
using App.DTOs;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/admin/users/all
    [HttpGet("users/all")]
    public async Task<ActionResult<AllUsersResponse>> GetAllUsers()
    {
        List<UserModel> users = await _context.Users.ToListAsync();

        AllUsersResponse response = new AllUsersResponse
        {
            Users = users.Select(user => new AuthResponse
            {
                UserId = user.Id,
                Username = user.UserName!
            }).ToList()
        };

        return Ok(response);
    }

    // DELETE: api/admin/ban/:id
    [HttpDelete("ban/{id}")]
    public async Task<ActionResult<BanUserResponse>> BanUser(int id)
    {
        UserModel? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        await _context.Posts
            .Where(post => post.PostedBy.Id == id || post.PostedOn.CreatedBy.Id == id)
            .ExecuteDeleteAsync();

        await _context.Threads
            .Where(thread => thread.CreatedBy.Id == id)
            .ExecuteDeleteAsync();

        _context.Users.Remove(user);

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        BanUserResponse response = new BanUserResponse
        {
            UserId = id
        };

        return Ok(response);
    }

    // DELETE: api/admin/post/:id
    [HttpDelete("post/{id}")]
    public async Task<ActionResult<PostDeletedResponse>> DeletePost(int id)
    {
        PostModel? post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound(new { message = "Cannot delete a post that does not exist." });
        }

        _context.Posts.Remove(post);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok(new PostDeletedResponse
        {
            PostId = post.Id
        });
    }

    // DELETE: api/admin/thread/:id
    [HttpDelete("thread/{id}")]
    public async Task<ActionResult<DeletedThreadResponse>> DeleteThread(int id)
    {
        ThreadModel? thread = await _context.Threads.FirstOrDefaultAsync(t => t.Id == id);

        if (thread == null)
        {
            return NotFound(new { message = "Cannot delete a thread that does not exist." });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        await _context.Posts
            .Where(post => post.PostedOn.Id == id)
            .ExecuteDeleteAsync();

        _context.Threads.Remove(thread);

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok(new DeletedThreadResponse
        {
            ThreadId = thread.Id
        });
    }
}