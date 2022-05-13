using System.ComponentModel.DataAnnotations;

namespace BookMS.Models.ViewModels
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "请输入邮箱")]
        [EmailAddress(ErrorMessage ="邮箱格式不正确")]
        public string Email { get; set; }
    }
}   
