using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
