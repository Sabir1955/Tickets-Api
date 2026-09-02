using Authentication.Data;
using Authentication.DTOs;
using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // =====================================================
        // REGISTER
        // POST: api/Auth/register
        // =====================================================

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return BadRequest("Email already exists.");
            }

            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(request.Password);

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
                message = "Registration successful",
                userId = user.Id,
                name = user.Name,
                email = user.Email
            });
        }

        // =====================================================
        // LOGIN
        // POST: api/Auth/login
        // =====================================================

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                return Unauthorized(
                    "Your account has been deactivated.");
            }

            bool passwordValid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash
                );

            if (!passwordValid)
            {
                return Unauthorized("Invalid email or password.");
            }

            string accessToken = GenerateAccessToken(user);

            string refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(
                    Convert.ToDouble(
                        _configuration["Jwt:RefreshTokenDays"]
                    )
                )
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Login successful",
                accessToken = accessToken,
                refreshToken = refreshToken
            });
        }

        // =====================================================
        // REFRESH TOKEN
        // POST: api/Auth/refresh
        // =====================================================

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            RefreshTokenDto request)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.Token == request.RefreshToken
                );

            if (refreshToken == null)
            {
                return Unauthorized("Invalid refresh token.");
            }

            if (refreshToken.RevokedAt != null)
            {
                return Unauthorized("Refresh token has been revoked.");
            }

            if (refreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return Unauthorized("Refresh token has expired.");
            }

            if (!refreshToken.User.IsActive)
            {
                return Unauthorized("User account is inactive.");
            }

            // Revoke old refresh token
            refreshToken.RevokedAt = DateTime.UtcNow;

            // Generate new tokens
            string newAccessToken =
                GenerateAccessToken(refreshToken.User);

            string newRefreshToken =
                GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = refreshToken.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(
                    Convert.ToDouble(
                        _configuration["Jwt:RefreshTokenDays"]
                    )
                )
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        // =====================================================
        // FORGOT PASSWORD
        // POST: api/Auth/forgot-password
        // =====================================================

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == request.Email
                );

            // Do not reveal whether email exists
            if (user == null)
            {
                return Ok(new
                {
                    message =
                        "If the email exists, a password reset token has been generated."
                });
            }

            // Generate secure reset token
            string token = GenerateSecureToken();

            var resetToken = new PasswordResetToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);

            await _context.SaveChangesAsync();

            // DEVELOPMENT ONLY
            // In production, send this token through email.
            return Ok(new
            {
                message =
                    "Password reset token generated.",
                resetToken = token
            });
        }

        // =====================================================
        // RESET PASSWORD
        // POST: api/Auth/reset-password
        // =====================================================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
    ResetPasswordRequestDto request)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == request.Token);

            if (resetToken == null)
            {
                return BadRequest("Invalid password reset token.");
            }

            if (resetToken.IsUsed)
            {
                return BadRequest("Password reset token has already been used.");
            }

            if (resetToken.ExpiresAt <= DateTime.UtcNow)
            {
                return BadRequest("Password reset token has expired.");
            }

            string newPasswordHash =
                BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            resetToken.User.PasswordHash = newPasswordHash;

            resetToken.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Password reset successful."
            });
        }
        // =====================================================
        // CHANGE PASSWORD
        // POST: api/Auth/change-password
        // =====================================================

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordDto request)
        {
            // Get logged-in user's ID from JWT
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId =
                int.Parse(userIdClaim.Value);

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Id == userId
                );

            if (user == null)
            {
                return NotFound("User not found.");
            }

            bool currentPasswordValid =
                BCrypt.Net.BCrypt.Verify(
                    request.CurrentPassword,
                    user.PasswordHash
                );

            if (!currentPasswordValid)
            {
                return BadRequest(
                    "Current password is incorrect."
                );
            }

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.NewPassword
                );

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Password changed successfully."
            });
        }

        // =====================================================
        // GENERATE ACCESS TOKEN
        // =====================================================

        private string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.Name
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email
                )
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                )
            );

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,

                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(
                        _configuration["Jwt:ExpiryMinutes"]
                    )
                ),

                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        // =====================================================
        // GENERATE REFRESH TOKEN
        // =====================================================

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64)
            );
        }

        // =====================================================
        // GENERATE PASSWORD RESET TOKEN
        // =====================================================

        private string GenerateSecureToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32)
            );
        }
    }
}