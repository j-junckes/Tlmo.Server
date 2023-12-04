using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Tlmo.Entities;

[Index(nameof(Slug), IsUnique = true)]
[Index(nameof(Name), IsUnique = false)]
public class Workspace
{
  [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public Guid Id { get; set; }

  [Required] public Guid OwnerId { get; set; }
  public User Owner { get; set; } = null!;

  [MinLength(3)]
  [MaxLength(16)]
  [RegularExpression("^[a-zA-Z0-9_]*$")]
  [Required]
  public string Slug { get; set; } = null!;

  [MinLength(3)]
  [MaxLength(32)]
  [Required]
  public string Name { get; set; } = null!;

  public List<User> Users { get; } = new();

  public List<Channel> Channels { get; } = new();

  [Required] public Instant CreatedAt { get; set; }
  [Required] public Instant LastUpdatedAt { get; set; }

  public Instant? DeletedAt { get; set; }
}