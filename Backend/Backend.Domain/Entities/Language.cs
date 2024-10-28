using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public partial class Language
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<UserLanguage> UserLanguages { get; set; } = new List<UserLanguage>();
}
