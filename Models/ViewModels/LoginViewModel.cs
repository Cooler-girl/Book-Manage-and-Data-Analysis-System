using System.ComponentModel.DataAnnotations;

namespace BookMS.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "用户名不能为空"), Display(Name = "账号")]
        [StringLength(30, ErrorMessage = "用户名为20个以内的数字或英文字母组成")]
        [RegularExpression(@"^(?=.*[a-zA-Z]|\d)[\da-zA-Z\@\.]{1,}$", ErrorMessage = "用户名为字母、数字、@、.的组合，且必须包含字母和数字。")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "密码不能为空"), Display(Name = "密码")]
        [StringLength(16, ErrorMessage = "密码必须为{2}到 {1} 个字符组成.", MinimumLength = 6)]
        [DataType(DataType.Password, ErrorMessage = "密码必须包含数字和小写字母")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])[\da-zA-Z\@\.\:\!\-\+\#\$\%\^\&]{6,16}$", ErrorMessage = "密码必须包含小写字母和数字")]
        public string PassWord { get; set; }
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
