using Authentication.Data;
using Authentication.DTOs.Users;
using Authentication.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 1. GET ALL USERS
        // GET: /api/v1/users
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }


        // =====================================================
        // 2. GET USER BY ID
        // GET: /api/v1/users/{id}
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(user);
        }


        // =====================================================
        // 3. CREATE USER
        // POST: /api/v1/users
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> CreateUser(
            CreateUserDto request)
        {
            // Check email
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return BadRequest(new
                {
                    message = "Email already exists."
                });
            }

            // Hash password
            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                IsActive = true
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User created successfully.",
                userId = user.Id,
                name = user.Name,
                email = user.Email,
                isActive = user.IsActive
            });
        }


        // =====================================================
        // 4. UPDATE USER
        // PUT: /api/v1/users/{id}
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            UpdateUserDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // Check whether email belongs to another user
            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email == request.Email &&
                    u.Id != id);

            if (emailExists)
            {
                return BadRequest(new
                {
                    message = "Email already exists."
                });
            }

            user.Name = request.Name;
            user.Email = request.Email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User updated successfully."
            });
        }


        // =====================================================
        // 5. DELETE USER
        // DELETE: /api/v1/users/{id}
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User deleted successfully."
            });
        }


        // =====================================================
        // 6. ACTIVATE USER
        // PATCH: /api/v1/users/{id}/activate
        // =====================================================

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            user.IsActive = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User activated successfully."
            });
        }


        // =====================================================
        // 7. DEACTIVATE USER
        // PATCH: /api/v1/users/{id}/deactivate
        // =====================================================

        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            user.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User deactivated successfully."
            });
        }
    }
}