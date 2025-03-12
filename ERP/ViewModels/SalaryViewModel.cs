namespace ERP.ViewModels
{
    public class SalaryViewModel
    {
        public int SalaryId { get; set; }
        public int? EmployeeId { get; set; }
        public bool IsClosed { get; set; }
        public DateOnly MonthPointer {  get; set; }
        public List<ProjectPaymentViewModel> ProjectPayments { get; set; }
        public decimal? FixSalary { get; set; }
        public decimal FinallyAmount { get; set; }
    }

    public class ProjectPaymentViewModel
    {
        public int projectPaymentId { get; set; }
        public bool? isStagerAdd { get; set; }
        public int projectId { get; set; }
        public string? projectTitle { get; set; }
        public int? partNumber { get; set; }
        public int? partsCount { get; set; }
        public decimal? amount { get; set; }
        public decimal? punishment { get; set; }
        public string? punishmentDescription { get; set; }
    }

    public class ProjectPaymentUpdateModel
    {
        public int EmployeeId { get; set; }
       
        public int ProjectPaymentId { get; set; }
        public decimal? Punishment { get; set; }
        public string? PunishmentDescription { get; set; }
    }
}
