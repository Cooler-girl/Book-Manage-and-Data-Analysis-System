using System.ComponentModel.DataAnnotations;

namespace BookMS.Models.ViewModels
{
    public class ForgotPasswordConfirmation
    {
        public string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public string EmailConfirmationUrl { get; set; }
    }
}
