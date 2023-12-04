using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using Tlmo.Repository;
using Tlmo.Server.Extensions;

namespace Tlmo.Server.Controllers.User;

[ApiController, Route("users")]
public class UserController(ILogger<UserController> logger, TlmoContext context, IConfiguration config) : ControllerBase
{
  [HttpPost("register", Name = "RegisterUser"), AllowAnonymous]
  public async Task<IActionResult> Register([FromBody] RegisterDto model, CancellationToken cToken)
  {
    await using var transaction = await context.Database.BeginTransactionAsync(cToken);

    var errors = new List<string>();

    try
    {
      if (await context.Users.AnyAsync(u => u.Username == model.Username, cToken))
      {
        errors.Add("error.register.username_taken");
      }

      if (await context.Users.AnyAsync(u => u.Email == model.Email, cToken))
      {
        errors.Add("error.register.email_taken");
      }

      if (errors.Any())
      {
        return BadRequest(new { messages = errors });
      }

      var salt = PasswordHelper.CreateSalt();
      var hash = PasswordHelper.HashPassword(model.Password, salt);

      if (hash == null)
      {
        throw new Exception("Error while hashing password");
      }

      var user = new Entities.User
      {
        Username = model.Username,
        Email = model.Email,
        PasswordType = "Argon2;HmacSha256",
        PasswordHash = Convert.ToBase64String(hash),
        PasswordSalt = Convert.ToBase64String(salt),
        LastConfirmedAt = SystemClock.Instance.GetCurrentInstant() // TODO: Send confirmation email
      };

      await context.Users.AddAsync(user, cToken);
      await context.SaveChangesAsync(cToken);

      await transaction.CommitAsync(cToken);

      return Ok(new { messages = new[] { "success.register" } });
    }
    catch (Exception e)
    {
      logger.LogError(e, "Error while registering user");
      await transaction.RollbackAsync(cToken);
      return StatusCode(500, new { messages = new[] { "error.register.unknown" } });
    }
  }

  [HttpPost("login", Name = "LoginUser"), AllowAnonymous]
  public async Task<IActionResult> Login([FromBody] LoginDto model, CancellationToken cToken)
  {
    await using var transaction = await context.Database.BeginTransactionAsync(cToken);
    try
    {
      var user = await context.Users.FirstOrDefaultAsync(a => a.Email == model.Email, cToken);
      if (user == null)
      {
        return BadRequest(new { messages = new[] { "error.login.invalid_credentials" } });
      }

      if (!PasswordHelper.VerifyHash(model.Password, Convert.FromBase64String(user.PasswordSalt),
            Convert.FromBase64String(user.PasswordHash)))
      {
        return BadRequest(new { messages = new[] { "error.login.invalid_credentials" } });
      }

      if (user.DeletedAt != null)
      {
        return BadRequest(new { messages = new[] { "error.login.account_deleted" } });
      }

      if (user.LastConfirmedAt == null)
      {
        return BadRequest(new { messages = new[] { "error.login.account_not_confirmed" } });
      }

      if (user.PasswordType == "Argon2;HmacSha256")
      {
        return Ok(new { token = await GenerateTokenAsync(user) });
      }

      throw new Exception($"Unknown password type of '{user.PasswordType}' for user '{user.Id}'");
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync(cToken);
      logger.LogError(ex, "Error while logging in user");
      return StatusCode(500, new { messages = new[] { "error.login" } });
    }
  }
  
  [HttpGet("me", Name = "GetUser"), Authorize]
  public async Task<ActionResult> Get(CancellationToken cToken)
  {
    await using var transaction = await context.Database.BeginTransactionAsync(cToken);
    try
    {
      var userId = this.GetUserId();
      var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cToken);
      if (user != null) return Ok(new MeDto
      {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        CreatedAt = user.CreatedAt.ToDateTimeUtc()
      });
      
      logger.LogError($"User '{userId}' not found");
      return NotFound(new { messages = new[] { "error.user.not_found" } });

    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync(cToken);
      logger.LogError(ex, "Error while getting user");
      return StatusCode(500, new { messages = new[] { "error.user.get" } });
    }
  }
  
  [HttpGet("{id}", Name = "GetUserById"), Authorize]
  public async Task<IActionResult> Get(Guid id, CancellationToken cToken)
  {
    await using var transaction = await context.Database.BeginTransactionAsync(cToken);
    try {
      var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id, cToken);
      if (user != null) return Ok(new UserDto
      {
        Id = user.Id,
        Username = user.Username,
        CreatedAt = user.CreatedAt.ToDateTimeUtc()
      });
      
      logger.LogError($"User '{id}' not found");
      return NotFound(new { messages = new[] { "error.user.not_found" } });

    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync(cToken);
      logger.LogError(ex, "Error while getting user");
      return StatusCode(500, new { messages = new[] { "error.user.get" } });
    }
  }

  private Task<string> GenerateTokenAsync(Entities.User user)
  {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? ""));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    var claims = new[]
    {
      new Claim("sub", user.Id.ToString())
    };

    var token = new JwtSecurityToken(config["Jwt:Issuer"] ?? "http://localhost:5000",
      config["Jwt:Audience"] ?? "http://localhost:5000",
      claims,
      expires: DateTime.Now.AddDays(1),
      signingCredentials: credentials);

    return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
  }
}