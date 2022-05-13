using System.ComponentModel.DataAnnotations;

namespace BookMS.Models
{
    public class Author
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="请输入作者姓名")]
        public string Name { get; set; }
        public string? Img { get; set; }//照片
        [Required(ErrorMessage ="请选择性别")]
        public char Sex { get; set; }
        public string? Country { get; set; } = "未知";//国籍
        public string? Nation { get; set; } = "未知";//民族
        public string? BirthPlace { get; set; } = "未知";//出生地
        [DisplayFormat(DataFormatString = "{0:yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage ="请选择出生日期")]
        public DateTime BirthYear { get; set; }
        public string? Introduce { get; set; }
        public string? Remark { get; set; }
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
