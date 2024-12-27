using System;
using System.Collections.Generic;

namespace ERP.Models;

public partial class Project
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public bool? IsArchived { get; set; }

    public string? Color { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public DateOnly? EventDate { get; set; }

    public DateOnly? Deadline { get; set; }

    public int? EmployeeId { get; set; }

    public int? ClientId { get; set; }

    public int? DeliveryToAddress { get; set; }

    public int? DeliveryToContact { get; set; }

    public decimal? PaymentTotal { get; set; }

    public decimal? AdvanceRate { get; set; }
    public string? Description { get; set; }
    public string? Journal { get; set; }

    public virtual Client? Client { get; set; }

    public virtual DeliveryAddress? DeliveryToAddressNavigation { get; set; }

    public virtual Contact? DeliveryToContactNavigation { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<ProjectFile>? ProjectFiles { get; set; } = new List<ProjectFile>();
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    public override string ToString()
    {
        return $"Project: {ProjectName} (ID: {ProjectId})\n" +
               $"Employee ID: {EmployeeId}\n" +
               $"Client ID: {ClientId}\n" +
               $"Payment Date: {PaymentDate?.ToString("yyyy-MM-dd") ?? "N/A"}\n" +
               $"Event Date: {EventDate?.ToString("yyyy-MM-dd") ?? "N/A"}\n" +
               $"Deadline: {Deadline?.ToString("yyyy-MM-dd") ?? "N/A"}\n" +
               $"Payment Total: {PaymentTotal?.ToString("C2") ?? "N/A"}\n" +
               $"Advance Rate: {AdvanceRate?.ToString("P2") ?? "N/A"}\n" +
               $"Delivery Address ID: {DeliveryToAddress}\n" +
               $"Delivery Contact ID: {DeliveryToContact}\n" +
               $"Items Count: {Items.Count}\n" +
               $"Files Count: {ProjectFiles?.Count ?? 0}";
    }

}
