using System.ComponentModel.DataAnnotations;

namespace BookMS.Models
{
    public class Book
    {
        [Required(ErrorMessage ="请输入图书编号")]
        [StringLength(12, ErrorMessage = "图书编号长度必须为12个字符.", MinimumLength = 12)]
        [RegularExpression(@"^[A-Z]{2}\d{10}$", ErrorMessage = "图书编号为两个大写字母加十个数字组成！")]
        public string BookID { get; set; }
        [Required(ErrorMessage ="请输入图书名")]
        public string Title { get; set; }//书名
        public string? Img { get; set; }//图书封面
        public Author? Author { get; set; }
        public string? Type { get; set; } = "未分类";//图书所属分类
        public Int64? Words { get; set; }//字数
        public string? Introduce { get; set; }//简介
        [Required(ErrorMessage ="请输入价格")]
        [RegularExpression(@"^[0-9]+(\.?[0-9]+)?$", ErrorMessage = "价格为数字")]
        public double Price { get; set; }//价格
        public int? Count { get; set; } = 0;//库存数量
        [Required(ErrorMessage = "请选择出版日期")]
        public DateTime PubDate { get; set; }//出版时间
        public string? Remark { get; set; }//备注
    }
}
