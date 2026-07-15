using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class EddyTrackingISContext : DbContext
    {
        public EddyTrackingISContext()
        {
        }

        public EddyTrackingISContext(DbContextOptions<EddyTrackingISContext> options)
            : base(options)
        {
        }

        public virtual DbSet<WidgetRequest> WidgetRequest { get; set; }
        public virtual DbSet<WidgetImpression> WidgetImpression { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //}
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WidgetRequest>(entity =>
            {
                entity.ToTable("WidgetRequest", "WS");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Ipaddress)
                    .HasColumnName("IPAddress")
                    .HasMaxLength(50);

                entity.Property(e => e.PageQueryString).HasMaxLength(4000);

                entity.Property(e => e.PageUrl)
                    .IsRequired()
                    .HasColumnName("PageURL")
                    .HasMaxLength(4000);

                entity.Property(e => e.ReferringQueryString).HasMaxLength(4000);

                entity.Property(e => e.ReferringUrl)
                    .HasColumnName("ReferringURL")
                    .HasMaxLength(4000);

                entity.Property(e => e.UserAgent).HasMaxLength(500);

                entity.Property(e => e.WidgetSettingsJson).HasMaxLength(4000);

                entity.Property(e => e.JqueryVersionNumber).HasMaxLength(10);
            });

            modelBuilder.Entity<WidgetImpression>(entity =>
            {
                entity.ToTable("WidgetImpression", "WS");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
