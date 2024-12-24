using System;
using System.Collections.Generic;

namespace ERP.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Passport { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string? Email { get; set; }
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public override string ToString()
    {
        return $"EmployeeId: {EmployeeId}, " +
               $"EmployeeName: {EmployeeName}, " +
               $"PhoneNumber: {PhoneNumber}, " +
               $"Passport: {Passport}, " +
               $"Position: {Position}, " +
               $"Email: {Email}, " +
               $"Number of Projects: {Projects.Count}";
    }
}
