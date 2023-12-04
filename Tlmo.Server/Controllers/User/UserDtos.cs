using System.ComponentModel.DataAnnotations;
using NodaTime;
using Tlmo.Server.Validators;

namespace Tlmo.Server.Controllers.User;

public class RegisterDto
{
  [MinLength(3, ErrorMessage = "Username should be at least 3 characters long")]
  [MaxLength(16, ErrorMessage = "Username should be at maximum 16 characters long")]
  [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Username can only contain letters, numbers and underscores")]
  [Required]
  public string Username { get; set; } = null!;

  [EmailAddress(ErrorMessage = "Invalid email address")]
  [Required]
  public string Email { get; set; } = null!;

  [HasDigit]
  [HasLowercase]
  [HasUppercase]
  [MinLength(8)]
  [Required]
  public string Password { get; set; } = null!;
}

public record LoginDto
{
  [EmailAddress(ErrorMessage = "Invalid email address")]
  [Required]
  public string Email { get; set; } = null!;

  [HasDigit]
  [HasLowercase]
  [HasUppercase]
  [MinLength(8)]
  [Required]
  public string Password { get; set; } = null!;
}

public record MeDto
{
  public Guid Id { get; set; }
  public string Username { get; set; } = null!;
  public string Email { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
}

public record UserDto
{
  public Guid Id { get; set; }
  public string Username { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
}
