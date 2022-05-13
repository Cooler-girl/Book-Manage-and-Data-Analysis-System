using BookMS.Data;
using BookMS.Help;
using BookMS.Models;
using BookMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookMS.Controllers
{
    [Authorize]
    public class AuthorsController : Controller
    {
        #region 依赖注入
        private readonly BookContext _context;

        public AuthorsController(BookContext context)
        {
            _context = context;
        }
        #endregion

        #region 作者列表
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var authors = from a in _context.Authors
                          select a;
            int pageSize = 12;//页尺寸
            return View(await PaginatedList<Author>.CreateAsync(authors.Include(m => m.Books).AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        #endregion

        #region 作者详情
        public async Task<IActionResult> Details(int? id)
        {
            Console.WriteLine("作者详情");
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.Include(a => a.Books)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (author == null)
            {
                return NotFound();
            }
            if (author.Books.Count() == 0)
            {
                Console.WriteLine("没有查到作品！");
            }
            return View(author);
        }
        #endregion

        #region 创建作者
        [Authorize(Roles = "管理员,超级管理员")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Img,Sex,Country,Nation,BirthPlace,BirthYear,Introduce,Remark")] Author author)
        {
            if (author.BirthYear >= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "出生日期不应晚于当前时间！");
                return View(author);
            }
            var au = await _context.Authors.FirstOrDefaultAsync(m => m.Name == author.Name);
            if (au != null)//有重名作者不能新建
            {
                ModelState.AddModelError(string.Empty, "作者名重复，请修改！");
                return View(author);
            }
            string path = "wwwroot/images/author/";
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
                    Console.WriteLine("ID: {0}\n姓名：{1}\n头像：{2}\n性别：{3}\n国籍：{4}\n民族：{5}\n出生地：{6}\n出生日期：{7}\n简介：{8}", author.Id, author.Name, author.Img, author.Sex, author.Country, author.Nation, author.BirthPlace, author.BirthYear, author.Introduce);
                    fileName = path + author.Name + ".jpg";

                    author.Img = "images/author/" + author.Name + ".jpg";
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
                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }
            return View(author);
        }
        #endregion

        #region 修改作者
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Img,Sex,Country,Nation,BirthPlace,BirthYear,Introduce,Remark")] Author author)
        {
            if (id != author.Id)
            {
                return NotFound();
            }
            if (author.BirthYear >= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "出生日期不应晚与当前时间");
                return View(author);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
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
            return View(author);
        }
        #endregion

        #region 删除作者
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author == null)
            {
                return NotFound();
            }

            var books = await _context.Books.Include(m => m.Author).Where(m => m.Author.Id == author.Id).AsNoTracking().ToListAsync();
            if (books.Any())
            {
                foreach (var book in books)
                {
                    _context.Books.Remove(book);
                }
                _context.SaveChanges();
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }
        #endregion

        #region 作者管理
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> Manage(string currentFilter, string searchString, int? pageNumber)
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

            var authors = from author in _context.Authors
                          select author;
            int pageSize = 6;
            if (!String.IsNullOrEmpty(searchString))
            {
                authors = authors.Where(a => a.Name.Contains(searchString));
            }
            return View(await PaginatedList<Author>.CreateAsync(authors.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        #endregion

        #region 辅助方法
        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(e => e.Id == id);
        }
        #endregion
    }
}
