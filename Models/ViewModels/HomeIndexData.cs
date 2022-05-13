namespace BookMS.Models.ViewModels
{
    public class HomeIndexData
    {
        public IEnumerable<Book>? Books { get; set; }
        public IEnumerable<Author>? Authors { get; set; }
    }
}
