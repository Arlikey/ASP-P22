using ASP_P22.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ASP_P22.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserAccess> Accesses { get; set; }

        public DataContext(DbContextOptions options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("site");
            modelBuilder.Entity<User>()
                .HasMany(u => u.Accesses)       
                .WithOne(ua => ua.User)            
                .HasForeignKey(ua => ua.UserId);

            modelBuilder.Entity<UserAccess>().HasIndex(a => a.Login).IsUnique();
        }
    }
}
