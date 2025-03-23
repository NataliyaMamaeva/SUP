using System.ComponentModel.DataAnnotations;

namespace ERP.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Token { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите новый пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Пароль должен содержать минимум одну заглавную букву, одну строчную, одну цифру и один спецсимвол.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Подтвердите пароль")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    
        public override string ToString()
        {
            return $"Token: {Token}, Email: {Email}, NewPassword: {NewPassword}, ConfirmPassword: {ConfirmPassword}";
        }

    }
}
