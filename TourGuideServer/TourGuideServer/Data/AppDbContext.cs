using Microsoft.EntityFrameworkCore;
using TourGuideServer.Models;

namespace TourGuideServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<POI> POIs { get; set; }
        public DbSet<POITranslation> POITranslations { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<VisitHistory> VisitHistories { get; set; }
        public DbSet<QRCode> QRCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Table mapping ───────────────────────────────────────────────
            modelBuilder.Entity<POI>().ToTable("POI");
            modelBuilder.Entity<POITranslation>().ToTable("POI_Translations");
            modelBuilder.Entity<QRCode>().ToTable("QRCode");
            modelBuilder.Entity<VisitHistory>().ToTable("VisitHistory");

            // ── Language PK ─────────────────────────────────────────────────
            modelBuilder.Entity<Language>()
                .HasKey(x => x.LanguageCode);

            // ── POITranslation PK ───────────────────────────────────────────
            modelBuilder.Entity<POITranslation>()
                .HasKey(t => t.TranslationID);

            modelBuilder.Entity<POITranslation>()
                .HasIndex(t => new { t.POIID, t.LanguageCode })
                .IsUnique();

            // ── POI → Translations (1-N) ───────────────────────────
            modelBuilder.Entity<POI>()
                .HasMany(p => p.Translations)
                .WithOne()
                .HasForeignKey(t => t.POIID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<POITranslation>()
                .HasOne<Language>()
                .WithMany()
                .HasForeignKey(t => t.LanguageCode)
                .OnDelete(DeleteBehavior.Restrict);

            // ── QRCode → POI (cascade) ───────────────────────────────────────
            modelBuilder.Entity<QRCode>()
                .HasOne(q => q.POI)
                .WithMany()
                .HasForeignKey(q => q.POIID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QRCode>()
                .HasIndex(q => q.QRValue)
                .IsUnique();

            // ── VisitHistory ─────────────────────────────────────────────────
            modelBuilder.Entity<VisitHistory>().HasKey(v => v.VisitID);

            modelBuilder.Entity<VisitHistory>()
                .Property(v => v.UserLat).HasColumnType("decimal(9,6)");
            modelBuilder.Entity<VisitHistory>()
                .Property(v => v.UserLon).HasColumnType("decimal(9,6)");

            // ── POI Lat/Lng precision ────────────────────────────────────────
            modelBuilder.Entity<POI>()
                .Property(p => p.Latitude).HasColumnType("decimal(9,6)");
            modelBuilder.Entity<POI>()
                .Property(p => p.Longitude).HasColumnType("decimal(9,6)");
        }
    }
}