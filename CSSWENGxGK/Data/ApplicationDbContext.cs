using Microsoft.EntityFrameworkCore;
using CSSWENGxGK.Models;

namespace CSSWENGxGK.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Volunteer> T_Volunteer { get; set; }
}
