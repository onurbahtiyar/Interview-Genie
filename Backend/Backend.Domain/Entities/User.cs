using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public virtual ICollection<InterviewSession> InterviewSessions { get; set; } = new List<InterviewSession>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<UserLanguage> UserLanguages { get; set; } = new List<UserLanguage>();

    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
}
