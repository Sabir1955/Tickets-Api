using Authentication.Data;
using Authentication.DTOs;

using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("api/v1/roles")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/roles
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles
                .OrderBy(r => r.Id)
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/v1/roles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound("Role not found.");
            }

            return Ok(role);
        }

        // POST: api/v1/roles
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleDto request)
        {
            // Check duplicate role
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.Name);

            if (existingRole != null)
            {
                return BadRequest("Role already exists.");
            }

            // Create role
            var role = new Role
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Role created successfully.",
                roleId = role.Id,
                name = role.Name,
                description = role.Description,
                isActive = role.IsActive,
                createdAt = role.CreatedAt
            });
        }

        // PUT: api/v1/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(
            int id,
            UpdateRoleDto request)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound("Role not found.");
            }

            // Check duplicate role name
            var duplicateRole = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.Name == request.Name &&
                    r.Id != id);

            if (duplicateRole != null)
            {
                return BadRequest(
                    "Another role with this name already exists."
                );
            }

            // Update role
            role.Name = request.Name;
            role.Description = request.Description;
            role.IsActive = request.IsActive;
            role.CreatedAt = role.CreatedAt;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Role updated successfully.",
                roleId = role.Id,
                name = role.Name,
                description = role.Description,
                isActive = role.IsActive
            });
        }

        // DELETE: api/v1/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound("Role not found.");
            }

            _context.Roles.Remove(role);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Role deleted successfully."
            });
        }
    }
}