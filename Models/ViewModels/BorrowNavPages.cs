using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookMS.Models.ViewModels
{
    public class BorrowNavPages
    {
        public static string TypePie => "TypePie";//图书类型借阅情况(饼图)
        public static string TypeBar => "TypeBar";//图书类型借阅情况(柱状图)
        public static string WeekBar => "WeekBar";//近七天借阅情况(柱状图)
        public static string WeekLine => "WeekLine";//近七天借阅情况(折线图)
        public static string MonthLine => "MonthLine";//近三十天借阅情况(折线图)

        public static string TypePieNavClass(ViewContext viewContext) => PageNavClass(viewContext, TypePie);
        public static string TypeBarNavClass(ViewContext viewContext) => PageNavClass(viewContext, TypeBar);
        public static string WeekBarNavClass(ViewContext viewContext) => PageNavClass(viewContext, WeekBar);
        public static string WeekLineNavClass(ViewContext viewContext) => PageNavClass(viewContext, WeekLine);
        public static string MonthLineNavClass(ViewContext viewContext) => PageNavClass(viewContext, MonthLine);

        public static string PageNavClass(ViewContext viewContext, string chart)
        {
            var activeChart = viewContext.ViewData["ActiveChart"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activeChart, chart, StringComparison.OrdinalIgnoreCase) ? "bg-info" : null;
        }
    }
}
