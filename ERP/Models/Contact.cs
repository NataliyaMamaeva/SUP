using System;
using System.Collections.Generic;

namespace ERP.Models;

public partial class Contact
{
    public int ContactId { get; set; }

    public int? ClientId { get; set; }

    public string? ContactName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Passport { get; set; }

    public virtual Client? Client { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
