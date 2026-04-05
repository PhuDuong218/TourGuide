using Microsoft.EntityFrameworkCore;
using TourGuideServer.Models;

namespace TourGuideServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<POI> POIs { get; set; }
        public DbSet<POITranslation> POI_Translations { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<VisitHistory> VisitHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<POI>().ToTable("POI");
            modelBuilder.Entity<POITranslation>().ToTable("POI_Translations");

            // Language PK
            modelBuilder.Entity<Language>()
                .HasKey(x => x.LanguageCode);

            modelBuilder.Entity<POITranslation>()
                .HasKey(t => new { t.POIID, t.LanguageCode });

            modelBuilder.Entity<POI>()
                .HasMany(p => p.Translations)
                .WithOne(t => t.POI)
                .HasForeignKey(t => t.POIID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<POITranslation>()
                .HasOne(t => t.Language)
                .WithMany()
                .HasForeignKey(t => t.LanguageCode)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Fix: Khai báo precision cho Latitude và Longitude
            // Tránh warning "silently truncated" và khớp với kiểu DECIMAL(9,6) trong SQL
            modelBuilder.Entity<POI>()
                .Property(p => p.Latitude)
                .HasColumnType("decimal(9,6)");

            modelBuilder.Entity<POI>()
                .Property(p => p.Longitude)
                .HasColumnType("decimal(9,6)");
        }
    }
}