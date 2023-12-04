using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Tlmo.Entities;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Username), IsUnique = true)]
public class User
{
  [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public Guid Id { get; set; }

  [Required, MinLength(3), MaxLength(16)]
  public string Username { get; set; } = null!;

  [Required] public string PasswordType { get; set; } = null!;
  [Required] public string PasswordHash { get; set; } = null!;
  [Required] public string PasswordSalt { get; set; } = null!;

  [Required, EmailAddress] public string Email { get; set; } = null!;
  
  public List<Workspace> Workspaces { get; } = new();
  
  public List<Workspace> OwnedWorkspaces { get; } = new();

  [Required] public Instant CreatedAt { get; set; }
  [Required] public Instant LastUpdatedAt { get; set; }

  public Instant? LastConfirmedAt { get; set; }
  public Instant? DeletedAt { get; set; }
}