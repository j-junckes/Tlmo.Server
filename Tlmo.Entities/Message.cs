using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Tlmo.Entities;

[Index(nameof(ChannelId), IsUnique = false)]
[Index(nameof(AuthorId), IsUnique = false)]
public class Message
{
  [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public Guid Id { get; set; }

  [Required] public Guid ChannelId { get; set; }
  public Channel Channel { get; set; } = null!;

  [Required] public Guid AuthorId { get; set; }
  public User Author { get; set; } = null!;

  [Required, MinLength(1)]
  public string Content { get; set; } = null!;

  [Required] public Instant CreatedAt { get; set; }
  [Required] public Instant LastUpdatedAt { get; set; }
}