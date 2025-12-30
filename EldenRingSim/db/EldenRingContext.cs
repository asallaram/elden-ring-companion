using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using EldenRingSim.Models;
using System;
using System.Linq;

namespace EldenRingSim.DB
{
    // Database context for the Elden Ring Simulator application.
    public class EldenRingContext : IdentityDbContext<ApplicationUser>
    {
        public EldenRingContext(DbContextOptions<EldenRingContext> options) : base(options) { }

        public DbSet<Ammo> Ammos { get; set; }
        public DbSet<Armor> Armors { get; set; }
        public DbSet<AshOfWar> AshesOfWar { get; set; }
        public DbSet<Bosses> Bosses { get; set; }
        public DbSet<BossStats> BossStats { get; set; }
        public DbSet<Classes> Classes { get; set; }
        public DbSet<Creatures> Creatures { get; set; }
        public DbSet<Incantations> Incantations { get; set; }
        public DbSet<Items> Items { get; set; }
        public DbSet<Locations> Locations { get; set; }
        public DbSet<NPCs> NPCs { get; set; }
        public DbSet<Shields> Shields { get; set; }
        public DbSet<Sorceries> Sorceries { get; set; }
        public DbSet<Spirits> Spirits { get; set; }
        public DbSet<Talismans> Talismans { get; set; }
        public DbSet<Weapons> Weapons { get; set; }
        public DbSet<PlayerProgress> PlayerProgress { get; set; }
        public DbSet<BossFightAttempt> BossFightAttempts { get; set; }
        public DbSet<BossFightSession> BossFightSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ammo>().OwnsMany(a => a.AttackPower);

            modelBuilder.Entity<Armor>().OwnsMany(a => a.DmgNegation);
            modelBuilder.Entity<Armor>().OwnsMany(a => a.Resistance);

            modelBuilder.Entity<AshOfWar>().OwnsOne(a => a.DescriptionDetails);

            modelBuilder.Entity<Bosses>().OwnsMany(b => b.Drops);
            modelBuilder.Entity<Classes>().OwnsMany(c => c.Stats);
            modelBuilder.Entity<Creatures>().OwnsMany(c => c.Drops);

            modelBuilder.Entity<Incantations>().OwnsMany(i => i.Effects);
            modelBuilder.Entity<Incantations>().OwnsMany(i => i.Requires);

            modelBuilder.Entity<Items>().OwnsMany(i => i.Effect);
            modelBuilder.Entity<Locations>().OwnsMany(l => l.SubLocations);

            modelBuilder.Entity<Shields>().OwnsMany(s => s.Attack);
            modelBuilder.Entity<Shields>().OwnsMany(s => s.Defence);
            modelBuilder.Entity<Shields>().OwnsMany(s => s.ScalesWith);
            modelBuilder.Entity<Shields>().OwnsMany(s => s.RequiredAttributes);

            modelBuilder.Entity<Sorceries>().OwnsMany(s => s.Effects);
            modelBuilder.Entity<Sorceries>().OwnsMany(s => s.Requires);

            modelBuilder.Entity<Spirits>().OwnsMany(s => s.Effect);

            modelBuilder.Entity<Weapons>().OwnsMany(w => w.Attack);
            modelBuilder.Entity<Weapons>().OwnsMany(w => w.Defence);
            modelBuilder.Entity<Weapons>().OwnsMany(w => w.ScalesWith);
            modelBuilder.Entity<Weapons>().OwnsMany(w => w.RequiredAttributes);

            modelBuilder.Entity<PlayerProgress>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.PlayerName);
                entity.HasIndex(p => p.UserId);

                entity.HasOne(p => p.User)
                      .WithMany(u => u.PlayerProgresses)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<BossFightSession>()
    .Property(s => s.WeaponsTriedIds)
    .HasConversion(
        v => string.Join(',', v),
        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
    );

        }
    }
}
