using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Models;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThreadsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ThreadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Threads
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ThreadModel>>> GetThreads()
        {
            return await _context.Threads.ToListAsync();
        }

        // GET: api/Threads/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ThreadModel>> GetThreadModel(int id)
        {
            var threadModel = await _context.Threads.FindAsync(id);

            if (threadModel == null)
            {
                return NotFound();
            }

            return threadModel;
        }

        // PUT: api/Threads/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutThreadModel(int id, ThreadModel threadModel)
        {
            if (id != threadModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(threadModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ThreadModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Threads
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ThreadModel>> PostThreadModel(ThreadModel threadModel)
        {
            _context.Threads.Add(threadModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetThreadModel", new { id = threadModel.Id }, threadModel);
        }

        // DELETE: api/Threads/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteThreadModel(int id)
        {
            var threadModel = await _context.Threads.FindAsync(id);
            if (threadModel == null)
            {
                return NotFound();
            }

            _context.Threads.Remove(threadModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ThreadModelExists(int id)
        {
            return _context.Threads.Any(e => e.Id == id);
        }
    }
}
