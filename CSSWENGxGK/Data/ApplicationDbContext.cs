using Microsoft.EntityFrameworkCore;
using CSSWENGxGK.Models;

namespace CSSWENGxGK.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    public DbSet<Volunteer> T_Volunteer { get; set; }
    public DbSet<User> T_User { get; set; }
    public DbSet<Event> T_Event {  get; set; }
    public DbSet<Organizer> T_Organizer { get; set; }
    public DbSet<EventsAttended> T_EventsAttended { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventsAttended>()
            .HasKey(e => new { e.EventID, e.VolunteerID });

        // Other entity configurations and relationships
    }
}
