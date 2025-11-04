using Microsoft.EntityFrameworkCore;

namespace Tychy.Components
{
    public class AppDbContext : DbContext
    {
        public DbSet<EbookPlatform> Platforms { get; set; }
        public DbSet<EbookCode> Codes { get; set; }
        public DbSet<CodeRequest> Requests { get; set; }
        public DbSet<Reader> Readers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EbookCode>()
                .HasOne(e => e.Platform)
                .WithMany(p => p.Codes)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CodeRequest>()
                .HasOne(r => r.Platform)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CodeRequest>()
                .HasOne(r => r.AssignedCode)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CodeRequest>()
                .HasOne(r => r.Reader)
                .WithMany(rd => rd.Requests)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
