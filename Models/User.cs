using Microsoft.AspNetCore.Identity;

namespace BookMS.Models
{
    public class User : IdentityUser
    {
        //自定义属性
        public string? Name { get; set; } = null;//名字
        public ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();

        // 以下四个属性是 Identity 内置的四个权限属性，照着添加上去再权限校验的时候用到。
        public ICollection<IdentityUserClaim<string>> Claims { get; set; }
        public ICollection<IdentityUserLogin<string>> Logins { get; set; }
        public ICollection<IdentityUserToken<string>> Tokens { get; set; }
        public ICollection<IdentityUserRole<string>> UserRoles { get; set; }

        public User()
        {
            Claims = new List<IdentityUserClaim<string>>();
            Logins = new List<IdentityUserLogin<string>>();
            Tokens = new List<IdentityUserToken<string>>();
            UserRoles = new List<IdentityUserRole<string>>();
        }
    }
}
