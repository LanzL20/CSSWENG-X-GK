using Microsoft.EntityFrameworkCore;
using CSSWENGxGK.Models.Domain;

namespace CSSWENGxGK.Data
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions options) : base(options) 
        { 

        }

        public DbSet<Volunteer> Volunteers { get; set; }
    }
}
