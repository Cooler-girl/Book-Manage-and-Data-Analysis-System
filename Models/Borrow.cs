namespace BookMS.Models
{
    public class Borrow
    {
        public int Id { get; set; }
        public string BookID { get; set; }
        public String UserID { get; set; }
        public DateTime BorrowDate { get; set; }//借出时间
        public DateTime BackDate { get; set; }//预计归还时间
        public string state { get; set; }//状态：借阅中、已归还
        public bool IsPosted { get; set; }//是否已提交管理员
        public User? User { get; set; }
        public Book? Book { get; set; }
    }
}
