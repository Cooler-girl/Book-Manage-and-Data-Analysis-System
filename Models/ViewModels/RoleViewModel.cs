using System.ComponentModel.DataAnnotations;

namespace BookMS.Models.ViewModels
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage ="角色名是需要的"), Display(Name = "角色")]
        public string Name { get; set; }
    }
}
