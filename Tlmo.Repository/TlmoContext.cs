using Microsoft.EntityFrameworkCore;
using Tlmo.Entities;

namespace Tlmo.Repository;

public class TlmoContext : DbContext
{
  public TlmoContext(DbContextOptions<TlmoContext> options) : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    #region User

    modelBuilder.Entity<User>()
      .Property(p => p.CreatedAt)
      .HasDefaultValueSql("now()")
      .ValueGeneratedOnAdd();

    modelBuilder.Entity<User>()
      .Property(p => p.LastUpdatedAt)
      .HasDefaultValueSql("now()")
      .ValueGeneratedOnAddOrUpdate();

    #endregion

    #region Workspace

    modelBuilder.Entity<Workspace>()
      .Property(p => p.CreatedAt)
      .HasDefaultValueSql("now()")
      .ValueGeneratedOnAdd();

    modelBuilder.Entity<Workspace>()
      .Property(p => p.LastUpdatedAt)
      .HasDefaultValueSql("now()")
      .ValueGeneratedOnAddOrUpdate();

    modelBuilder.Entity<Workspace>()
      .HasMany(e => e.Users)
      .WithMany(e => e.Workspaces);

    modelBuilder.Entity<Workspace>()
      .HasOne(e => e.Owner)
      .WithMany(e => e.OwnedWorkspaces)
      .HasForeignKey(e => e.OwnerId)
      .IsRequired();

    #endregion

    #region Channel

    modelBuilder.Entity<Channel>()
      .Property(p => p.CreatedAt)
      .HasDefaultValueSql("now()")
      .ValueGeneratedOnAdd();
    
    modelBuilder.Entity<Channel>()
      .Property(p => p.LastUpdatedAt)
      .HasDefaultValueSql("now()")
      .ValueGeneratedOnAddOrUpdate();
    
    modelBuilder.Entity<Channel>()
      .HasOne(e => e.Workspace)
      .WithMany(e => e.Channels)
      .HasForeignKey(e => e.WorkspaceId)
      .IsRequired();

    modelBuilder.Entity<Channel>()
      .HasMany(e => e.Messages)
      .WithOne(e => e.Channel)
      .HasForeignKey(e => e.ChannelId)
      .IsRequired();

    #endregion

    #region Message

    modelBuilder.Entity<Message>()
      .Property(p => p.CreatedAt)
      .HasDefaultValueSql("now()")
      .ValueGeneratedOnAdd();
    
    modelBuilder.Entity<Message>()
      .Property(p => p.LastUpdatedAt)
      .HasDefaultValueSql("now()")
      .ValueGeneratedOnAddOrUpdate();
    
    modelBuilder.Entity<Message>()
      .HasOne(e => e.Channel)
      .WithMany(e => e.Messages)
      .HasForeignKey(e => e.ChannelId)
      .IsRequired();
    
    modelBuilder.Entity<Message>()
      .HasOne(e => e.Author)
      .WithMany()
      .HasForeignKey(e => e.AuthorId)
      .IsRequired();
    
    #endregion
  }

  public DbSet<User> Users { get; set; } = null!;

  public DbSet<Workspace> Workspaces { get; set; } = null!;

  public DbSet<Channel> Channels { get; set; } = null!;

  public DbSet<Message> Messages { get; set; } = null!;
}