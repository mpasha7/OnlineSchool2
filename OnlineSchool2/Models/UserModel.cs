using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Models
{
    public class UserModel
    {
        //[Required]
        //public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        public string OldPassword { get; set; }
       
        public string? NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "Новые пароли не совпадают")]
        public string? ConfirmPassword { get; set; }
    }
}
