using Microsoft.EntityFrameworkCore;
using CSSWENGxGK.Models;

namespace CSSWENGxGK.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<User> T_User { get; set; }
}
