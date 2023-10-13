using Game.DataAccess.Context.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Game.DataAccess.Context;

public class GameDbContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<ResourceType> ResourceTypes { get; set; }
    public DbSet<Gift> Gifts { get; set; }

    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique constraint for DeviceId in Players table
        modelBuilder.Entity<Player>()
            .HasIndex(p => p.DeviceId)
            .IsUnique();

        // Player Configuration
        modelBuilder.Entity<Player>()
            .Property(p => p.DeviceId)
            .HasMaxLength(255)
            .IsRequired();

        // Composite unique constraint for Resources on PlayerId and ResourceTypeId
        modelBuilder.Entity<Resource>()
            .HasIndex(r => new { r.PlayerId, r.ResourceTypeId })
            .IsUnique();

        modelBuilder.Entity<Resource>()
            .Property(s => s.ResourceValue)
            .IsConcurrencyToken();

        modelBuilder.Entity<Resource>()
            .HasCheckConstraint("CK_Resource_ResourceValue", "ResourceValue >= 0");

        // Setting up the relationships for Player with Resource
        modelBuilder.Entity<Player>()
            .HasMany(p => p.Resources)
            .WithOne(r => r.Player)
            .HasForeignKey(r => r.PlayerId);

        // Setting up the relationships for ResourceType with Resource
        modelBuilder.Entity<ResourceType>()
            .HasMany(rt => rt.Resources)
            .WithOne(r => r.ResourceType)
            .HasForeignKey(r => r.ResourceTypeId);

        // Setting up the relationships for Player with Gifts (Sent)
        modelBuilder.Entity<Player>()
            .HasMany(p => p.SentGifts)
            .WithOne(g => g.Sender)
            .HasForeignKey(g => g.SenderPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Setting up the relationships for Player with Gifts (Received)
        modelBuilder.Entity<Player>()
            .HasMany(p => p.ReceivedGifts)
            .WithOne(g => g.Receiver)
            .HasForeignKey(g => g.ReceiverPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Setting up the relationships for ResourceType with Gifts
        modelBuilder.Entity<ResourceType>()
            .HasMany(rt => rt.Resources)
            .WithOne(r => r.ResourceType)
            .HasForeignKey(r => r.ResourceTypeId);

        // ResourceType Configuration
        modelBuilder.Entity<ResourceType>()
            .Property(rt => rt.ResourceTypeName)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<ResourceType>().HasData(
            new ResourceType { Id = 1, ResourceTypeName = "Coins" },
            new ResourceType { Id = 2, ResourceTypeName = "Rolls" }
        );
    }
}