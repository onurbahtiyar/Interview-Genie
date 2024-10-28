using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class InterviewSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid CompanyInfoId { get; set; }

    public bool IsActive { get; set; }
    
    public string? ProfileComment { get; set; }

    public virtual CompanyInfo CompanyInfo { get; set; } = null!;

    public virtual ICollection<InterviewQuestion> InterviewQuestions { get; set; } = new List<InterviewQuestion>();

    public virtual User User { get; set; } = null!;
}
