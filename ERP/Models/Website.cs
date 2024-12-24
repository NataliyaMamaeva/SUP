using System;
using System.Collections.Generic;

namespace ERP.Models;

public partial class Website
{
    public string Website1 { get; set; } = null!;

    public int? ClientId { get; set; }

    public virtual Client? Client { get; set; }
}
