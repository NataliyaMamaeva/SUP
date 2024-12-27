namespace ERP.ViewModels
{
    public class CurrentProjectsViewModel
    {
        public List<EmployeeProjectsViewModel> Employees { get; set; } = new List<EmployeeProjectsViewModel>();
    }

    public class EmployeeProjectsViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public List<ProjectCardViewModel> Projects { get; set; } = new List<ProjectCardViewModel>();
    }

}
