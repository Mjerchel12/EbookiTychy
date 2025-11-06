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
                .WithOne(rd => rd.Request)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EbookPlatform>()
                .Property(p => p.Instructions)
                .IsRequired(false);

            modelBuilder.Entity<EbookPlatform>()
                .Property(p => p.Name)
                .IsRequired(false);

            modelBuilder.Entity<EbookCode>()
                .Property(p => p.Code)
                .IsRequired(false);

            modelBuilder.Entity<EbookCode>()
                .Property(p => p.LastModifiedBy)
                .IsRequired(false);

            modelBuilder.Entity<CodeRequest>()
                .Property(p => p.Email)
                .IsRequired(false);

            modelBuilder.Entity<CodeRequest>()
                .Property(p => p.ValidationMessage)
                .IsRequired(false);

            modelBuilder.Entity<CodeRequest>()
                .Property(p => p.RejectionReason)
                .IsRequired(false);

            modelBuilder.Entity<Reader>()
                .Property(p => p.Email)
                .IsRequired(false);

            modelBuilder.Entity<Reader>()
                .Property(p => p.FullName)
                .IsRequired(false);

            modelBuilder.Entity<Reader>()
                .Property(p => p.BlockReason)
                .IsRequired(false);

            modelBuilder.Entity<Reader>()
                .HasOne(r => r.Request)
                .WithOne(req => req.Reader)
                .HasForeignKey<CodeRequest>(req => req.ReaderId);
        }

    }
}
