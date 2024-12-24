using System;
using System.Collections.Generic;

namespace ERP.Models;

public partial class TaskItem
{
    public int TaskId { get; set; }

    public int ProjectId { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly Deadline { get; set; }

    public string? Description { get; set; }

    public virtual Employee Employee { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
}
