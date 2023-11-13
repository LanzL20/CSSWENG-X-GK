using CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CSSWENGxGK.Data;
public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    public DbSet<Volunteer> T_Volunteer { get; set; }
    public DbSet<Event> T_Event {  get; set; }
    public DbSet<Organizer> T_Organizer { get; set; }
    public DbSet<EventsAttended> T_EventsAttended { get; set; }
    public DbSet<Role> T_Role { get; set; }
    public DbSet<User> T_User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<EventsAttended>().HasKey(e => new { e.EventID, e.VolunteerID });
        modelBuilder.Entity<IdentityUserLogin<int>>().HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId });
    }
}
