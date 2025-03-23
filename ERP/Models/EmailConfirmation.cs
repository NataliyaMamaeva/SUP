namespace ERP.Models
{
    public class EmailConfirmation
    {
        public int EmailConfirmationId { get; set; }
        public string Email {  get; set; }
        public string Token {  get; set; }
        public int EmployeeId {  get; set; }

        public virtual Employee Employee { get; set; }
    }
}
