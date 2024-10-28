using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class UserSkill
{
    public Guid UserId { get; set; }

    public Guid SkillId { get; set; }

    public Guid Id { get; set; }

    public virtual Skill Skill { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
