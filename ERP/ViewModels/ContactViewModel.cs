namespace ERP.ViewModels
{
    public class ClientViewModel
    {
        public int? ClientId { get; set; }
        public string? ClientTitle { get; set; }
        public string? City { get; set; }
        public DateOnly? FirstContact { get; set; }
        public List<ContactViewModel>? contacts { get; set; }
        public List<AddressViewModel>? address { get; set; }
        public List<RekvizitViewModel>? rekvizits { get; set; }

        public override string ToString()
        {
            var contactsString = contacts != null && contacts.Any()
                ? string.Join(", ", contacts.Select(c => $"[ID: {c.ContactId}, Name: {c.Name}, Phone: {c.Phone}, Email: {c.Email}]"))
                : "Нет контактов";

            var addressString = address != null && address.Any()
                ? string.Join(", ", address.Select(a => $"[Address:{a.AddressId},  {a.Address}]"))
                : "Нет адресов";

            var rekvizitString = rekvizits != null && rekvizits.Any()
                ? string.Join(", ", rekvizits.Select(f => f.FilePath?.FileName))
                : "Нет файлов реквизитов";

            return $"ClientViewModel:\n" +
                   $"- ClientId: {ClientId}\n" +
                   $"- ClientTitle: {ClientTitle}\n" +
                   $"- City: {City ?? "Не указан"}\n" +
                   $"- FirstContact: {FirstContact?.ToString("yyyy-MM-dd") ?? "Не указан"}\n" +
                   $"- Contacts: {contactsString}\n" +
                   $"- Addresses: {addressString}\n"+
                   $"- Rekvizit Files: {rekvizitString}";
        }

    }

    public class ContactViewModel
    {
        public int ContactId { get; set; }
        public string Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class AddressViewModel
    {
        public int AddressId { get; set; }
        public string Address { get; set; }
    }

    public class RekvizitViewModel
    {
        public int RekvizitId { get; set; }
        public string? ClientTitle { get; set; }
        public string? FileName { get; set; }
        public IFormFile? FilePath { get; set; }
    }

}
