using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Organiser)
                .WithOne(o => o.User)
                .HasForeignKey<Organiser>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Participant)
                .WithOne(p => p.User)
                .HasForeignKey<Participant>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Organiser -> Events
            modelBuilder.Entity<Organiser>()
                .HasMany(o => o.Events)
                .WithOne(e => e.Organiser)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event precision for Distance
            modelBuilder.Entity<Event>()
                .Property(e => e.Distance)
                .HasColumnType("decimal(8,2)");

            // Category precision
            modelBuilder.Entity<Category>()
                .Property(c => c.Distance)
                .HasColumnType("decimal(8,2)");

            modelBuilder.Entity<Category>()
                .Property(c => c.EntryFee)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Category>()
                .Property(c => c.LateEntryFee)
                .HasColumnType("decimal(10,2)");

            // Event -> Categories and Enrolments cascade
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Categories)
                .WithOne(c => c.Event)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Enrolments)
                .WithOne(en => en.Event)
                .HasForeignKey(en => en.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Category -> Enrolments
            // Restrict (NO ACTION): Event already cascades to Enrolments directly AND via
            // Categories. SQL Server rejects multiple cascade paths, so one must not cascade.
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Enrolments)
                .WithOne(en => en.Category)
                .HasForeignKey(en => en.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Participant -> Enrolments
            // Restrict (NO ACTION): User would otherwise reach Enrolments through both
            // Organiser -> Events -> Enrolments and Participant -> Enrolments (multiple cascade paths).
            // Delete a participant's enrolments explicitly before deleting the participant.
            modelBuilder.Entity<Participant>()
                .HasMany(p => p.Enrolments)
                .WithOne(en => en.Participant)
                .HasForeignKey(en => en.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrolment -> Result (one-to-one)
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Result)
                .WithOne(r => r.Enrolment)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
