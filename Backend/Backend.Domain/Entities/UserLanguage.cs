using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class UserLanguage
{
    public Guid UserId { get; set; }

    public Guid LanguageId { get; set; }

    public Guid Id { get; set; }

    public virtual Language Language { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
