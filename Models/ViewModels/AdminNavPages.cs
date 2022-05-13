using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookMS.Models.ViewModels
{
    public class AdminNavPages
    {
        public static string BookManage => "BookManage";//图书管理
        public static string UserManage => "UserManage";//用户管理
        public static string AuthorManage => "AuthorManage";//作者管理
        public static string AnalysisManage => "AnalysisManage";//图表
        public static string OverDueManage => "OverDueManage";//逾期管理
        public static string AdminManage => "AdminManage";//给用户分配管理员角色
        public static string RoleManage => "RoleManage";//角色管理

        public static string BookNavClass(ViewContext viewContext) => PageNavClass(viewContext, BookManage);
        public static string UserNavClass(ViewContext viewContext) => PageNavClass(viewContext, UserManage);
        public static string AuthorNavClass(ViewContext viewContext) => PageNavClass(viewContext, AuthorManage);
        public static string AnalysisNavClass(ViewContext viewContext) => PageNavClass(viewContext, AnalysisManage);
        public static string OverDueNavClass(ViewContext viewContext) => PageNavClass(viewContext, OverDueManage);
        public static string AdminNavClass(ViewContext viewContext) => PageNavClass(viewContext, AdminManage);
        public static string RoleNavClass(ViewContext viewContext) => PageNavClass(viewContext, RoleManage);

        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
