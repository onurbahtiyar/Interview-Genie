using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class Project
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();

    public virtual User User { get; set; } = null!;
}
