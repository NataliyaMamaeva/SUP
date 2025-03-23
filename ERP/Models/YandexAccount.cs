namespace ERP.Models
{


    public class YandexAccount
    {
        public int Id { get; set; }               // Уникальный ID
        public string Email { get; set; } = "";   // Email аккаунта
        public string AccessToken { get; set; } = "";  // Текущий токен доступа
        public string RefreshToken { get; set; } = ""; // Токен для обновления доступа
        public DateTime ExpiryDate { get; set; }  // Дата истечения токена
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Дата создания
        public bool IsCurrent { get; set; }

    }
}
