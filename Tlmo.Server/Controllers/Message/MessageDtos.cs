using System.ComponentModel.DataAnnotations;

namespace Tlmo.Server.Controllers.Message;

public record CreateMessageDto 
{
  [MinLength(1)]
  [MaxLength(1024)]
  [Required]
  public string Content { get; init; } = string.Empty;
}

public record CreateMessageResponseDto
{
  public Guid Id { get; init; }
  public string Content { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
  public DateTime LastUpdatedAt { get; init; }
  public Guid ChannelId { get; init; }
  public Guid AuthorId { get; init; }
}