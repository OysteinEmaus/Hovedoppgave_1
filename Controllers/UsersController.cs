using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Hovedoppgave.Data;
using Hovedoppgave.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hovedoppgave.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hjelpemdetode for å hente ut bruker-ID fra JWT-token
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new Exception("User ID not found in token");
            }
            return int.Parse(userIdClaim.Value);
        }

        // GET: /Users/current
        [HttpGet("current")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            try
            {
                var userId = GetUserId();
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Ikke return password hash
                user.PasswordHash = null;

                return user;
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: /Users (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();

            // Fjern password hash fra alle brukere
            foreach (var user in users)
            {
                user.PasswordHash = null;
            }

            return users;
        }

        // PUT: /Users/current
        [HttpPut("current")]
        public async Task<IActionResult> UpdateCurrentUser(UserUpdateRequest updateRequest)
        {
            try
            {
                var userId = GetUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Oppdater kun de feltene som er angitt i forespørselen
                if (!string.IsNullOrEmpty(updateRequest.Email))
                {
                    // Sjekk om e-post allerede er i bruk
                    if (await _context.Users.AnyAsync(u => u.Email == updateRequest.Email && u.Id != userId))
                    {
                        return BadRequest(new { message = "Email is already taken" });
                    }
                    user.Email = updateRequest.Email;
                }

                // Oppdater passord hvis det er angitt
                if (!string.IsNullOrEmpty(updateRequest.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateRequest.Password);
                }

                await _context.SaveChangesAsync();

                // Ikke return password hash
                user.PasswordHash = null;

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: /Users/current
        [HttpDelete("current")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            try
            {
                var userId = GetUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Ekstra endepunkt for admin til å oppdatere en bruker
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO til bruk for oppdatering av bruker
    public class UserUpdateRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}