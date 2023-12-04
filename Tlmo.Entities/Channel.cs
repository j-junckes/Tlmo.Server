using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Tlmo.Entities;

[Index(nameof(Slug), IsUnique = false)]
[Index(nameof(WorkspaceId), IsUnique = false)]
public class Channel
{
  [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public Guid Id { get; set; }

  [Required] public Guid WorkspaceId { get; set; }
  public Workspace Workspace { get; set; } = null!;

  public List<Message> Messages { get; } = new();

  [MinLength(3)]
  [MaxLength(16)]
  [Required]
  public string Slug { get; set; } = null!;
  
  [Required] public Instant CreatedAt { get; set; }
  [Required] public Instant LastUpdatedAt { get; set; }
}