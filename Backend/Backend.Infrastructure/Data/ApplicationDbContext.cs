using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace Backend.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<CompanyInfo> CompanyInfos { get; set; }
        public DbSet<InterviewSession> InterviewSessions { get; set; }
        public DbSet<InterviewQuestion> InterviewQuestions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tüm DateTime özelliklerine UTC ValueConverter uygulayın
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v, // Kaydederken
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // Okurken

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v, // Kaydederken
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v); // Okurken

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }

            // Diğer entity ayarları...

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<CompanyInfo>().ToTable("CompanyInfo");
            modelBuilder.Entity<InterviewSession>().ToTable("InterviewSession");
            modelBuilder.Entity<InterviewQuestion>().ToTable("InterviewQuestion");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<InterviewSession>()
                .HasMany(s => s.Questions)
                .WithOne(q => q.InterviewSession)
                .HasForeignKey(q => q.InterviewSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InterviewSession>(entity =>
            {
                entity.Property(e => e.StartedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now() at time zone 'utc'");

                entity.Property(e => e.EndedAt)
                    .HasColumnType("timestamp with time zone");
            });

            modelBuilder.Entity<InterviewQuestion>(entity =>
            {
                entity.Property(e => e.AskedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now() at time zone 'utc'");

                entity.Property(e => e.AnsweredAt)
                    .HasColumnType("timestamp with time zone");
            });

            modelBuilder.Entity<InterviewQuestion>()
                .Property(q => q.QuestionType)
                .HasConversion<string>()
                .IsRequired();

            modelBuilder.Entity<InterviewQuestion>()
                .Property(q => q.Options)
                .HasColumnType("text[]");
        }
    }
}
