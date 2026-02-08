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

namespace App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ThreadsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ThreadsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/threads/all
    [HttpGet("all")]
    public async Task<ActionResult<AllThreadsResponse>> GetAllThreads()
    {
        List<ThreadModel> threads = await _context.Threads.Include(t => t.CreatedBy).ToListAsync();

        AllThreadsResponse response = new AllThreadsResponse
        {
            Threads = threads.Select(thread => new ThreadResponse
            {
                ThreadId = thread.Id,
                Title = thread.Title,
                Published = thread.Published,
                InitialMessage = thread.InitialMessage,
                CreatedBy = thread.CreatedBy.UserName!
            }).ToList()
        };

        return Ok(response);
    }

    // GET: api/threads/thread/:id
    [HttpGet("thread/{id}")]
    public async Task<ActionResult<ThreadResponse>> GetThread(int id)
    {
        ThreadModel? thread = await _context.Threads.Include(t => t.CreatedBy).Where(thread => thread.Id == id).FirstOrDefaultAsync();

        if (thread == null)
        {
            return NotFound(new { message = $"Thread with ID:{id} does not exist." });
        }

        if (thread.CreatedBy == null)
        {
            return BadRequest(new { message = "This thread does not belong to anyone." });
        }

        return Ok(new ThreadResponse
        {
            ThreadId = thread.Id,
            Title = thread.Title,
            Published = thread.Published,
            InitialMessage = thread.InitialMessage,
            CreatedBy = thread.CreatedBy.UserName!
        });
    }

    // POST: api/threads/thread/new
    [Authorize]
    [HttpPost("thread/new")]
    public async Task<ActionResult<NewThreadResponse>> NewThread(NewThreadRequest request)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        UserModel? createdBy = await _context.Users.FindAsync(userId);

        if (createdBy == null)
        {
            return NotFound(new { message = "User not found." });
        }

        ThreadModel thread = new ThreadModel
        {
            Title = request.Title,
            InitialMessage = request.InitialMessage,
            Published = DateTime.Now,
            CreatedBy = createdBy
        };

        _context.Threads.Add(thread);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { message = "An error occurred while saving the thread.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }

        return Ok(new NewThreadResponse
        {
            Success = true,
            ThreadId = thread.Id
        });
    }

    // PUT: api/threads/thread/:id
    [Authorize]
    [HttpPut("thread/{id}")]
    public async Task<ActionResult<UpdateThreadResponse>> UpdateThread(UpdateThreadRequest request, int id)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        ThreadModel? oldThread = await _context.Threads.Include(t => t.CreatedBy).Where(thread => thread.Id == id).FirstOrDefaultAsync();

        if (oldThread == null)
        {
            return NotFound(new { message = "Cannot edit a thread that does not exist." });
        }

        if (oldThread.CreatedBy.Id != userId)
        {
            return Unauthorized(new { message = "You cannot edit a thread that belongs to another user." });
        }

        oldThread.Title = request.Title;
        oldThread.InitialMessage = request.InitialMessage;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ThreadModelExists(id))
            {
                return NotFound(new { message = "Cannot edit thread that does not exist." });
            }
            return StatusCode(409, new { message = "A concurrency error occurred while updating the thread." });
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { message = "Database error while updating thread.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }

        return Ok(new UpdateThreadResponse
        {
            ThreadId = oldThread.Id,
            Title = oldThread.Title,
            InitialMessage = oldThread.InitialMessage
        });
    }

    // DELETE: api/threads/thread/:id
    [Authorize]
    [HttpDelete("thread/{id}")]
    public async Task<ActionResult<DeletedThreadResponse>> DeleteThread(int id)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        ThreadModel? thread = await _context.Threads.Include(t => t.CreatedBy).Where(thread => thread.Id == id).FirstOrDefaultAsync();

        if (thread == null)
        {
            return NotFound(new { message = "Cannot delete a thread that does not exist." });
        }

        if (thread.CreatedBy.Id != userId)
        {
            return Unauthorized(new { message = "You cannot delete a thread that belongs to another user." });
        }

        await _context.Posts.Where(p => p.PostedOn.Id == id).ExecuteDeleteAsync();
        _context.Threads.Remove(thread);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new { message = "Database error while deleting a thread.", error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
        }

        return Ok(new DeletedThreadResponse
        {
            ThreadId = thread.Id
        });
    }

    private bool ThreadModelExists(int id)
    {
        return _context.Threads.Any(e => e.Id == id);
    }
}

