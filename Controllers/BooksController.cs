using BookMS.Data;
using BookMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookMS.Models.ViewModels;
using BookMS.Help;
using Microsoft.AspNetCore.Authorization;

namespace BookMS.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        #region 依赖注入
        private readonly BookContext _context;

        public BooksController(BookContext context)
        {
            _context = context;
        }
        #endregion

        #region 图书列表
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["AuthorSortParm"] = sortOrder == "Author" ? "author_desc" : "Author";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["WordsSortParm"] = sortOrder == "Words" ? "words_desc" : "Words";
            ViewData["CurrentFilter"] = searchString;//搜索框内容

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var books = from b in _context.Books
                        select b;

            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString) || b.Author.Name.Contains(searchString));
            }/*
            else
            {
                return RedirectToRoute(new { controller = "Home",Action = "Index"});
            }*/
            switch (sortOrder)
            {
                case "title_desc":
                    books = books.OrderByDescending(b => b.Title);
                    break;
                case "Author":
                    books = books.OrderBy(b => b.Author.Name);
                    break;
                case "author_desc":
                    books = books.OrderByDescending(b => b.Author.Name);
                    break;
                case "Date":
                    books = books.OrderBy(b => b.PubDate);
                    break;
                case "date_desc":
                    books = books.OrderByDescending(b => b.PubDate);
                    break;
                case "Words":
                    books = books.OrderBy(b => b.Words);
                    break;
                case "words_desc":
                    books = books.OrderByDescending(b => b.Words);
                    break;
                default:
                    books = books.OrderBy(b => b.Title);
                    break;
            }

            int pageSize = 4;
            return View(await PaginatedList<Book>.CreateAsync(books.Include(b => b.Author).AsNoTracking(), pageNumber ?? 1, pageSize));//books.Include(b=>b.Author).AsNoTracking().ToListAsync()
        }
        #endregion

        #region 图书详情
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }
        #endregion

        #region 创建图书
        [Authorize(Roles = "管理员,超级管理员")]
        public IActionResult Create()
        {
            ViewData["Author"] = new SelectList(_context.Authors, "Name", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? author, [Bind("BookID,Title,Img,Type,Words,Introduce,Price,Count,PubDate,Remark")] Book book)
        {
            if (book.PubDate >= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "出版日期应不晚于当前日期！");
                return View();
            }
            string path = "wwwroot/images/book/";
            //保存目录不存在就创建这个目录
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileName;

            var file = Request.Form.Files["file"];

            if (file != null)
            {
                if (file.Length > 0)
                {
                    int count = file.FileName.Split('.').Length;//统计.将名字分为几个部分
                    string exp = file.FileName.Split('.')[count - 1];//最后一部分为后缀名
                    Console.WriteLine("文件名：{0}\n文件扩展名：{1}", file.FileName, exp);
                    Console.WriteLine("书名: {0}\n图书编号：{1}\n封面：{2}\n分类：{3}\n字数：{4}\n简介：{5}\n价格：{6}\n库存：{7}\n出版日期：{8}", book.Title, book.BookID, book.Img, book.Type, book.Words, book.Introduce, book.Price, book.Count, book.PubDate);
                    fileName = path + book.Title + ".jpg";

                    book.Img = "images/book/" + book.Title + ".jpg";
                    FileHelper.CreateFile(fileName);

                    using (var stream = new FileStream(fileName, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "图片上传错误，请重新上传！");
                return View();
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine("模型有效！");
                if (author != null)
                {
                    var au = await _context.Authors.FirstOrDefaultAsync(m => m.Name == author);
                    book.Author = au;
                }
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }
            return View(book);
        }
        #endregion

        #region 修改图书
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.Include(m => m.Author).FirstOrDefaultAsync(m => m.BookID == id);
            if (book == null)
            {
                return NotFound();
            }
            if(book.Author == null)
            {
                book.Author = new Author ();
            }
            ViewData["Author"] = new SelectList(_context.Authors, "Name", "Name", book.Author.Name);
           
            
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string? author, [Bind("BookID,Title,Img,Type,Words,Introduce,Price,Count,PubDate,Remark")] Book book)
        {
            if (book.PubDate >= DateTime.Now)
            {                
                ModelState.AddModelError(string.Empty, "出版日期应不晚于当前日期！");
                return await Edit(book.BookID);
            }
            if (id != book.BookID)
            {
                return NotFound();
            }
            if (author != null)
            {
                var au = await _context.Authors.FirstOrDefaultAsync(m => m.Name == author);
                book.Author = au;
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Manage));
            }
            return View(book);
        }
        #endregion

        #region 删除图书
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var book = await _context.Books.FindAsync(id);
            if(book == null)
            {
                return NotFound();
            }
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }
        #endregion

        #region 图书管理
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> Manage(string currentFilter,string searchString, int? pageNumber)
        {
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;

            int pageSize = 6;

            if (!String.IsNullOrEmpty(searchString))
            {
                var books = from b in _context.Books.Include(m=>m.Author)
                            select b;
                books = books.Where(b => b.Title.Contains(searchString) || b.Author.Name.Contains(searchString));
                return View(await PaginatedList<Book>.CreateAsync(books.AsNoTracking(), pageNumber ?? 1, pageSize));
            }
            else
            {
                var books = _context.Books.Include(m => m.Author);
                return View(await PaginatedList<Book>.CreateAsync(books.AsNoTracking(), pageNumber ?? 1, pageSize));
            }            
        }
        #endregion

        #region 辅助方法
        private bool BookExists(string id)
        {
            return _context.Books.Any(e => e.BookID == id);
        }
        #endregion
    }
}
