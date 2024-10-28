using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class ProjectSkill
{
    public Guid ProjectId { get; set; }

    public Guid SkillId { get; set; }

    public Guid Id { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
