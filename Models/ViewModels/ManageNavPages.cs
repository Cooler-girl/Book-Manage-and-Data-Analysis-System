using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookMS.Models.ViewModels
{
    public class ManageNavPages
    {
        public static string Edit => "Edit";//我的资料
        public static string Borrow => "Borrow";//我的借阅
        public static string ChangePassword => "ChangePassword";//修改密码
        public static string LogOff => "LogOff";//注销


        public static string EditNavClass(ViewContext viewContext) => PageNavClass(viewContext, Edit);
        public static string BorrowNavClass(ViewContext viewContext) => PageNavClass(viewContext, Borrow);
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);
        public static string LogOffNavClass(ViewContext viewContext) => PageNavClass(viewContext, LogOff);

        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
