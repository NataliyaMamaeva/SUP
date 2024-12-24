using System;
using System.Collections.Generic;

namespace ERP.Models;

public partial class DeliveryAddress
{
    public int AddressId { get; set; }

    public int? ClientId { get; set; }

    public string DeliveryAddress1 { get; set; } = null!;

    public virtual Client? Client { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
