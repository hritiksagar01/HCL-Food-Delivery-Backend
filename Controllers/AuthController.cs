using HCL_Food_Delivery.Contracts;
using HCL_Food_Delivery.Data;
using HCL_Food_Delivery.Models;
using HCL_Food_Delivery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCL_Food_Delivery.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(FoodDeliveryDbContext dbContext, IJwtTokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = tokenService.CreateToken(user);
        return Ok(new AuthResponse(token, new AuthUserResponse(user.Id, user.Name, user.Email, user.Role.ToString())));
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail);
        if (exists)
        {
            return Conflict("User already exists with this email.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        var token = tokenService.CreateToken(user);
        return Ok(new AuthResponse(token, new AuthUserResponse(user.Id, user.Name, user.Email, user.Role.ToString())));
    }
}

