using ERP.Models;

namespace ERP.ViewModels
{

    public enum Position
    {
        master,
        designer,
        manager,
        boss
    }

    public class CreateEmployeeViewModel
    {

        public string EmployeeName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string? Passport { get; set; } = null!;

        public string Position { get; set; } = null!;

        public string? Email { get; set; }

        public string Password { get; set; } = null!;

        public override string ToString()
        {
            return $"EmployeeName: {EmployeeName}, " +
                   $"PhoneNumber: {PhoneNumber}, " +
                   $"Passport: {Passport}, " +
                   $"Position: {Position}, " +
                   $"Email: {Email}, " +
                   $"Password: {Password}";
        }
    }
}
