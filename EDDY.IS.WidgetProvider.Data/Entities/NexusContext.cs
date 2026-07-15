using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EDDY.IS.WidgetProvider.Data.Entities
{
    public partial class NexusContext : DbContext
    {
        public NexusContext()
        {
        }

        public NexusContext(DbContextOptions<NexusContext> options)
            : base(options)
        {
        }

        public virtual DbSet<VendorWidget> VendorWidget { get; set; }
        public virtual DbSet<VendorWidgetSettingValue> VendorWidgetSettingValue { get; set; }
        public virtual DbSet<Widget> Widget { get; set; }
        public virtual DbSet<Campaign> Campaigns { get; set; }
        public virtual DbSet<WidgetComponent> WidgetComponent { get; set; }
        public virtual DbSet<WidgetSetting> WidgetSetting { get; set; }
        public virtual DbSet<WidgetSettingType> WidgetSettingType { get; set; }
        public virtual DbSet<VwVendorWidgetConfiguration> VwVendorWidgetConfiguration { get; set; }
        public virtual DbSet<VwQDFTemplateConfiguration> VwQDFTemplateConfiguration { get; set; }
        public virtual DbSet<VendorWidgetUrlParameterConfig> VendorWidgetUrlParameterConfig { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.ToTable("Campaign", "dbo");
                entity.HasKey(e => e.CampaignId);
            });

            modelBuilder.Entity<VendorWidget>(entity =>
            {
                entity.ToTable("VendorWidget", "WS");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.VendorWidgetName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Widget)
                    .WithMany(p => p.VendorWidget)
                    .HasForeignKey(d => d.WidgetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VendorWidget_Widget");
            });

            modelBuilder.Entity<VendorWidgetSettingValue>(entity =>
            {
                entity.ToTable("VendorWidgetSettingValue", "WS");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                //entity.Property(e => e.Value).IsRequired();

                entity.HasOne(d => d.VendorWidget)
                    .WithMany(p => p.VendorWidgetSettingValue)
                    .HasForeignKey(d => d.VendorWidgetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VendorWidgetSettingValue_VendorWidget");

                entity.HasOne(d => d.WidgetSetting)
                    .WithMany(p => p.VendorWidgetSettingValue)
                    .HasForeignKey(d => d.WidgetSettingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VendorWidgetSettingValue_WidgetSetting");
            });

            modelBuilder.Entity<Widget>(entity =>
            {
                entity.ToTable("Widget", "WS");

                entity.Property(e => e.WidgetId).ValueGeneratedNever();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.WidgetName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<WidgetComponent>(entity =>
            {
                entity.ToTable("WidgetComponent", "WS");

                entity.Property(e => e.WidgetComponentId).ValueGeneratedNever();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.WidgetComponentName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<WidgetSetting>(entity =>
            {
                entity.ToTable("WidgetSetting", "WS");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.WidgetSettingKey)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.WidgetComponent)
                    .WithMany(p => p.WidgetSetting)
                    .HasForeignKey(d => d.WidgetComponentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WidgetSetting_WidgetComponent");

                entity.HasOne(d => d.Widget)
                    .WithMany(p => p.WidgetSetting)
                    .HasForeignKey(d => d.WidgetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WidgetSetting_Widget");

                entity.HasOne(d => d.WidgetSettingType)
                    .WithMany(p => p.WidgetSetting)
                    .HasForeignKey(d => d.WidgetSettingTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WidgetSetting_WidgetSettingType");
            });

            modelBuilder.Entity<WidgetSettingType>(entity =>
            {
                entity.ToTable("WidgetSettingType", "WS");

                entity.Property(e => e.WidgetSettingTypeId).ValueGeneratedNever();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.WidgetSettingTypeName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<VwVendorWidgetConfiguration>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_VendorWidgetConfiguration", "WS");

                //entity.Property(e => e.SettingValue).IsRequired();

                entity.Property(e => e.VendorWidgetName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.WidgetSettingKey)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VwQDFTemplateConfiguration>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_QDFTemplateConfiguration", "WS");

            });

            modelBuilder.Entity<VendorWidgetUrlParameterConfig>(entity =>
            {
                entity.ToTable("VendorWidgetUrlParameterConfig", "WS");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
