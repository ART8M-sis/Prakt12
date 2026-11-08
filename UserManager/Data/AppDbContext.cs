using Microsoft.EntityFrameworkCore;
using UserManager.Models;

namespace UserManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);
                entity.Property(u => u.Email)
                .IsUnicode();
                entity.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

                entity.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

                entity.HasQueryFilter(u => !u.IsDeleted);
            });
        }
    }
}
