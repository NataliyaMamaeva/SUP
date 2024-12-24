namespace ERP.ViewModels
{
    public class ClientViewModel
    {
        public string ClientTitle { get; set; }
        public string? City { get; set; }
        public DateOnly? FirstContact { get; set; }
        public List<ContactViewModel>? contacts { get; set; }
        public List<AddressViewModel>? address { get; set; }
        public List<RekvizitViewModel>? rekvizits { get; set; }

        public override string ToString()
        {
            var contactsString = contacts != null && contacts.Any()
                ? string.Join(", ", contacts.Select(c => $"[Name: {c.Name}, Phone: {c.Phone}, Email: {c.Email}]"))
                : "Нет контактов";

            var addressString = address != null && address.Any()
                ? string.Join(", ", address.Select(a => $"[Address: {a.Address}]"))
                : "Нет адресов";

            var rekvizitString = rekvizits != null && rekvizits.Any()
                ? string.Join(", ", rekvizits.Select(f => f.FilePath.FileName))
                : "Нет файлов реквизитов";

            return $"ClientViewModel:\n" +
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
        public string Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class AddressViewModel
    {
        public string Address { get; set; }
    }

    public class RekvizitViewModel
    {
        public string? ClientTitle { get; set; }
        public IFormFile? FilePath { get; set; }
    }

}
