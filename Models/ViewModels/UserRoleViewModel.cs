namespace BookMS.Models.ViewModels
{
    public class UserRoleViewModel
    {
        public string RoleId { get; set; }
        public string UserId { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
