using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class Skill
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();

    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
}
