using System;
using System.Collections.Generic;
using Backend.Domain.Entities;
using Backend.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<CompanyInfo> CompanyInfos { get; set; }

    public virtual DbSet<InterviewQuestion> InterviewQuestions { get; set; }

    public virtual DbSet<InterviewSession> InterviewSessions { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectSkill> ProjectSkills { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserLanguage> UserLanguages { get; set; }

    public virtual DbSet<UserSkill> UserSkills { get; set; }

    public virtual DbSet<LearningTree> LearningTree { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=160.20.109.155;Port=5432;Database=interview;Username=postgres;Password=Asc16rr5asc!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Companies_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.Position).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Companies)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Companies_UserId_fkey");
        });

        modelBuilder.Entity<CompanyInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CompanyInfo_pkey");

            entity.ToTable("CompanyInfo");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Industry).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(255);
        });

        modelBuilder.Entity<InterviewQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("InterviewQuestion_pkey");

            entity.ToTable("InterviewQuestion");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.InterviewSession).WithMany(p => p.InterviewQuestions)
                .HasForeignKey(d => d.InterviewSessionId)
                .HasConstraintName("InterviewQuestion_InterviewSessionId_fkey");

            entity.Property(e => e.Options)
                .HasColumnType("text[]");

            entity.Property(e => e.QuestionType)
                .HasConversion<string>();
        });

        modelBuilder.Entity<InterviewSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("InterviewSession_pkey");

            entity.ToTable("InterviewSession");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CompanyInfo).WithMany(p => p.InterviewSessions)
                .HasForeignKey(d => d.CompanyInfoId)
                .HasConstraintName("InterviewSession_CompanyInfoId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.InterviewSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("InterviewSession_UserId_fkey");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Languages_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Projects_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Projects)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Projects_UserId_fkey");
        });

        modelBuilder.Entity<ProjectSkill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProjectSkill_pkey");

            entity.ToTable("ProjectSkill");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectSkills)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("ProjectSkill_ProjectId_fkey");

            entity.HasOne(d => d.Skill).WithMany(p => p.ProjectSkills)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("ProjectSkill_SkillId_fkey");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Skills_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.HasIndex(e => e.Username, "Users_Username_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<UserLanguage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserLanguages_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");

            entity.HasOne(d => d.Language).WithMany(p => p.UserLanguages)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("UserLanguages_LanguageId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserLanguages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("UserLanguages_UserId_fkey");
        });

        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserSkills_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");

            entity.HasOne(d => d.Skill).WithMany(p => p.UserSkills)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("UserSkills_SkillId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserSkills)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("UserSkills_UserId_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
