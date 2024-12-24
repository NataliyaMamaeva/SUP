using System;
using System.Collections.Generic;

namespace ERP.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public string Title { get; set; } = null!;

    public string? City { get; set; }

    public DateOnly? FirstRequstDate { get; set; }

    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    public virtual ICollection<DeliveryAddress> DeliveryAddresses { get; set; } = new List<DeliveryAddress>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Requisite> Requisites { get; set; } = new List<Requisite>();
}
