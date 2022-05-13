using Microsoft.AspNetCore.Mvc;

namespace BookMS.Models.ViewModels
{
    public class ConfirmEmailModel
    {
        [TempData]
        public string StatusMessage { get; set; }
    }
}
