using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class CompanyInfo
{
    public Guid Id { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Industry { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<InterviewSession> InterviewSessions { get; set; } = new List<InterviewSession>();
}
