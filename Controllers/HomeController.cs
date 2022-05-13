using BookMS.Data;
using BookMS.Models;
using BookMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookMS.Controllers
{
    public class HomeController : Controller
    {
        #region 依赖注入
        private readonly ILogger<HomeController> _logger;
        private readonly BookContext _context;

        public HomeController(ILogger<HomeController> logger, BookContext context)
        {
            _logger = logger;
            _context = context;
        }
        #endregion

        #region 主页
        public async Task<IActionResult> Index()
        {
            HomeIndexData viewModel = new HomeIndexData();
            /*var borrows = from borrow in _context.Borrows
                          group borrow by borrow.BookID into g
                          orderby g.Count() descending
                          select new { bookId = g.Key };
            Console.WriteLine("按图书编号分组后的分组数量：{0}", borrows.Count());

            List<Book> books = new();
            foreach (var item in borrows)
            {
                var book = await _context.Books.FirstOrDefaultAsync(m => m.BookID == item.bookId);
                if(book != null)
                {
                    books.Add(book); 
                }
            }
            if (books.Count()>=5)
            {
                Console.WriteLine("借阅记录查出的数据足够，使用此数据！");
                viewModel.Books= books;
            }
            else
            {
                Console.WriteLine("借阅记录查出的数据不够，直接从书库获取数据！");*/
                viewModel.Books = await _context.Books.Include(b => b.Author).OrderByDescending(m=>m.Title).AsNoTracking().ToListAsync();
            //}
            viewModel.Authors = await _context.Authors.Include(a => a.Books).OrderByDescending(m => m.Name).AsNoTracking().ToListAsync();
            return View(viewModel);
        }
        #endregion

        #region 隐私政策
        public IActionResult Privacy()
        {
            return View();
        }
        #endregion

        #region 联系我们
        public IActionResult About()
        {
            return View();
        }
        #endregion

        #region 错误页
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion
    }
}