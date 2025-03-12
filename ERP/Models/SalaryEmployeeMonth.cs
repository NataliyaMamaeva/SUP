namespace ERP.Models
{

    public partial class SalaryEmployeeMonth
    {
        public bool isClosed { get; set; }
        public int SalaryId { get; set; }
        public int? EmployeeId { get; set; }
        public DateOnly MonthPointer { get; set; }
        public decimal? FixSalary { get; set; }
        public decimal? FinallyAmount { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<ProjectPayment> ProjectPayments { get; set; } = new List<ProjectPayment>();
        public override string ToString()
        {
            return $"SalaryEmployeeMonth: " +
                   $"isClosed={isClosed}, " +
                   $"SalaryId={SalaryId}, " +
                   $"EmployeeId={(EmployeeId.HasValue ? EmployeeId.Value.ToString() : "null")}, " +
                   $"MonthPointer={MonthPointer}, " +
                   $"FixSalary={(FixSalary.HasValue ? FixSalary.Value.ToString("F2") : "null")}, " +
                   $"FinallyAmount={(FinallyAmount.HasValue ? FinallyAmount.Value.ToString("F2") : "null")}, " +
                   $"Employee={(Employee != null ? Employee.ToString() : "null")}, " +
                   $"ProjectPaymentsCount={ProjectPayments.Count}";
        }
    }

    public partial class ProjectPayment
    {
        public int ProjectPaymentId { get; set; }
        public DateOnly MonthPointer { get; set; }
        public bool? isStagerAdd { get; set; } // если это надбавка с проекта стажёра
        public int? SalaryId { get; set; }
        public int? ProjectId { get; set; }
        public int? PartNumber { get; set; }
        public int? PartsCount { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Punishment { get; set; }
        public string? PunishmentDescription { get; set; }
        public virtual Project? Project { get; set; }
        public virtual SalaryEmployeeMonth? Salary { get; set; }
        public override string ToString()
        {
            return $"ProjectPayment: " +
                   $"ProjectPaymentId={ProjectPaymentId}, " +
                   $"MonthPointer={MonthPointer}, " +
                   $"SalaryId={(SalaryId.HasValue ? SalaryId.Value.ToString() : "null")}, " +
                   $"ProjectId={(ProjectId.HasValue ? ProjectId.Value.ToString() : "null")}, " +
                   $"PartNumber={(PartNumber.HasValue ? PartNumber.Value.ToString() : "null")}, " +
                   $"PartsCount={(PartsCount.HasValue ? PartsCount.Value.ToString() : "null")}, " +
                   $"Amount={(Amount.HasValue ? Amount.Value.ToString("F2") : "null")}, " +
                   $"Punishment={(Punishment.HasValue ? Punishment.Value.ToString("F2") : "null")}, " +
                   $"PunishmentDescription={(PunishmentDescription != null ? PunishmentDescription : "null")}, " +
                   $"Project={(Project != null ? Project.ProjectName: "null")}, " +
                   $"Salary={(Salary != null ? Salary.ToString() : "null")}, " +
                   $"isStagerAdd!!!!!!!!!!!!!!!!!!!!= {isStagerAdd}";
        }
    }
}
