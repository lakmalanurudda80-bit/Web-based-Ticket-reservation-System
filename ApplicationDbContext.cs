using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace EventTicketSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // DbSets
        public DbSet<Event> Events { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Organizer> Organizers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Remove cascade conventions to prevent multiple cascade paths
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            // Configure relationships with no cascade delete
            modelBuilder.Entity<Organizer>()
                .HasRequired(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Booking>()
                .HasRequired(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Event>()
                .HasRequired(e => e.Organizer)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganizerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Event>()
                .HasRequired(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Event>()
                .HasRequired(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BookingDetail>()
                .HasRequired(bd => bd.Booking)
                .WithMany(b => b.BookingDetails)
                .HasForeignKey(bd => bd.BookingId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BookingDetail>()
                .HasRequired(bd => bd.Ticket)
                .WithMany(t => t.BookingDetails)
                .HasForeignKey(bd => bd.TicketId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Ticket>()
                .HasRequired(t => t.Event)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EventId)
                .WillCascadeOnDelete(false);
        }
    }
}