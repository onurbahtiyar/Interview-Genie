using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class Company
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Position { get; set; }

    public string? Description { get; set; }

    public virtual User User { get; set; } = null!;
}
