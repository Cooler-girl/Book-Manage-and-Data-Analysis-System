using BookMS.Data;
using BookMS.Models;
using BookMS.Models.HelpModels;
using BookMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BookMS.Controllers
{
    [Authorize]
    public class BorrowsController : Controller
    {
        #region 依赖注入
        private readonly BookContext _context;
        private readonly UserManager<User> _userManager;

        public BorrowsController(BookContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        #endregion

        #region 借阅
        public async Task<IActionResult> Borrow(string bookId)
        {
            if (bookId == null)
            {
                return NotFound();
            }
            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookID == bookId);
            if (book == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            var userName = await _userManager.GetUserNameAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            //Console.WriteLine("当前用户：{0}\n用户ID：{2}\n查询到的书名：{1}",userName,book.Title,userId);
            Borrow borrow = new Borrow { BookID = book.BookID, UserID = userId, BorrowDate = DateTime.Now, BackDate = DateTime.Now.AddMonths(3), Book = book };
            return View(borrow);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow([Bind("BookID,UserID,BorrowDate,BackDate")] Borrow borrow)
        {
            if (borrow.BackDate <= DateTime.Now)
            {
                ModelState.AddModelError(String.Empty, "归还日期不能早于当前日期");
                var b = await _context.Books
                    .FirstOrDefaultAsync(m => m.BookID == borrow.BookID);
                borrow.Book = b;
                return await Borrow(borrow.BookID);
            }
            ModelState.Clear();
            Console.WriteLine("借阅\nBookID:{0}\nUserID:{1}\n借阅日期；{2}\n归还日期{3}", borrow.BookID, borrow.UserID, borrow.BorrowDate, borrow.BackDate);
            var overDue = await _context.Borrows.Include(m => m.Book).Include(m => m.User).SingleOrDefaultAsync(m => (m.UserID == borrow.UserID) && (m.BackDate < DateTime.Now) && (m.state == "借阅中"));
            if (overDue != null)
            {
                Console.WriteLine("逾期记录：\n图书:{0}\n图书编号:{1}\n用户：{2}\n借阅日期：{3}\n归还日期：{4}", overDue.Book.Title, overDue.BookID, overDue.User.Name, overDue.BorrowDate, overDue.BackDate);
                return RedirectToAction(nameof(Failed));
            }
            if (ModelState.IsValid)
            {
                Console.WriteLine("模型有效！");
                if (borrow.BackDate <= borrow.BorrowDate)
                {
                    ModelState.AddModelError(String.Empty,"归还日期不能早于借阅日期");
                    var b= await _context.Books
                        .FirstOrDefaultAsync(m => m.BookID == borrow.BookID);
                    borrow.Book = b;
                    return View(borrow);
                }
                borrow.state = "借阅中";

                var book = await _context.Books.FirstOrDefaultAsync(m => m.BookID == borrow.BookID);
                book.Count -= 1;
                _context.Update(book);
                await _context.SaveChangesAsync();

                _context.Borrows.Add(borrow);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyBorrows));
            }
            else
            {
                Console.WriteLine("模型无效！");
            }
            return View(borrow);
        }
        #endregion

        #region 借阅失败（有逾期记录）
        public IActionResult Failed()
        {
            return View();
        }
        #endregion

        #region 我的借阅记录
        public async Task<IActionResult> MyBorrows(int? pageNumber)
        {
            var user = await _userManager.GetUserAsync(User);//获取当前已登录用户
            var userId = await _userManager.GetUserIdAsync(user);
            var borrows = _context.Borrows.Include(b => b.Book).Include(b => b.User).Where(m => m.UserID == userId);
            int pageSize = 5;
            return View(await PaginatedList<Borrow>.CreateAsync(borrows.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        #endregion

        #region 归还
        public async Task<IActionResult> GiveBack(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var borrow = await _context.Borrows.Include(m => m.Book).Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);
            if (borrow == null)
            {
                return NotFound();
            }
            return View(borrow);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiveBack(int id, [Bind("Id,BookID,UserID,BorrowDate,BackDate,state")] Borrow borrow)
        {
            if (id != borrow.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                Console.WriteLine("模型有效。");
                borrow.state = "已归还";
                var book = await _context.Books.FindAsync(borrow.BookID);
                book.Count += 1;
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    _context.Update(borrow);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowExists(borrow.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(MyBorrows));
            }
            else
            {
                Console.WriteLine("模型无效！");
            }
            return View(borrow);
        }
        #endregion

        #region 提交
        public async Task<IActionResult> Submit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var borrow = await _context.Borrows.Include(m => m.Book).Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);
            if (borrow == null)
            {
                return NotFound();
            }
            return View(borrow);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id, [Bind("Id,BookID,UserID,BorrowDate,BackDate,state")] Borrow borrow)
        {
            Console.WriteLine("状态：{0}", borrow.state);
            if (id != borrow.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                Console.WriteLine("模型有效。");
                borrow.IsPosted = true;
                try
                {
                    _context.Update(borrow);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowExists(borrow.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(MyBorrows));
            }
            else
            {
                Console.WriteLine("模型无效！");
            }
            return View(borrow);
        }
        #endregion

        #region 管理
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> Manage(int? pageNumber)
        {
            var borrows = _context.Borrows.Include(m => m.Book).Include(m => m.User).Where(m => m.IsPosted == true);
            int pageSize = 5;
            return View(await PaginatedList<Borrow>.CreateAsync(borrows.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        #endregion

        #region 处理逾期
        [Authorize(Roles = "管理员,超级管理员")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Handle(int id)
        {
            var borrow = await _context.Borrows.FindAsync(id);
            if (borrow == null)
            {
                return NotFound();
            }
            borrow.state = "已归还";
            borrow.IsPosted = false;
            await _context.SaveChangesAsync();

            var book = await _context.Books.FindAsync(borrow.BookID);
            if(book == null)
            {
                return NotFound();
            }
            book.Count += 1;
            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                _context.Update(borrow);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BorrowExists(borrow.Id))
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
        #endregion

        #region 图书类型借阅情况
        public async Task<IActionResult> GetTypePie() {
            var borrows = await _context.Borrows.Include(m => m.Book).AsNoTracking().ToListAsync();
            var query = from b in borrows
                        group b by b.Book.Type;
            var model = new List<DataViewModel>();
            foreach (var item in query)
            {
                model.Add(new DataViewModel { Name = item.Key, Value = item.Count() });
            }
            return View(model);
        }

        public IActionResult GetTypeBar()
        {
            return View();
        }

        public async Task<IActionResult> GetTypeData()
        {
            var borrows = await _context.Borrows.Include(m => m.Book).AsNoTracking().ToListAsync();
            var query = from b in borrows
                        group b by b.Book.Type;
            var types = new List<string>();
            var counts = new List<int>();
            foreach (var item in query)
            {
                types.Add(item.Key);
                counts.Add(item.Count());
            }
            var x = JsonConvert.SerializeObject(types);
            var value = JsonConvert.SerializeObject(counts);
            string json = $"{{\"x\":{x},\"value\":{value}}}";

            return Json(json);
        }
        #endregion

        #region 近七天借阅情况
        public IActionResult GetWeekLine()
        {           
            return View();
        }

        public IActionResult GetWeekBar()
        {
            return View();
        }

        public IActionResult GetWeekData()
        {
            var borrows =GetBorrows();           
            var todayLastTime = GetTodayLastTime();
            var startDate = todayLastTime.AddDays(-7);
            var dates = GetDates(startDate,7);
            var groupData = GetGroupDataWithNoType(borrows, startDate, todayLastTime);
            var dateInGroup = new List<string>();
            var countInGroup = new List<int>();       
            foreach (var item in groupData)
            {
                dateInGroup.Add(item.Date);
                countInGroup.Add(item.Count);
            }
            Console.WriteLine("七天数据：");
            foreach(var item in groupData)
            {
                Console.WriteLine(item.Date + ":"+item.Count);
            }
            List<int> countInReturnResult = GetInitialList(7);//初始化线的数据
            for (int i = 0; i < 7; i++)
            {
                if (dateInGroup.Contains(dates[i]))
                {
                    int queryResultIndex = dateInGroup.IndexOf(dates[i]);
                    countInReturnResult[i] = countInGroup[queryResultIndex];                  
                }
            }              

            var x= JsonConvert.SerializeObject(dates);
            var value= JsonConvert.SerializeObject(countInReturnResult);
            string json = $"{{\"x\":{x},\"value\":{value}}}";
            return Json(json);
        }
        #endregion

        #region 近三十天借阅情况
        public IActionResult GetMonthLine()
        {
            return View();
        }

        public IActionResult GetMonthData()
        {
            var borrows = GetBorrows();
            var todayLastTime = GetTodayLastTime();
            var startDate = todayLastTime.AddDays(-30);
            var dates = GetDates(startDate, 30);
            var groupData = GetGroupData(borrows, startDate, todayLastTime);
            var groupTypeData = GetGroupTypeData(groupData);
            var values = new List<string>();//存储多条线的数据值
            List<int> totalCount = GetInitialList(30);//存储借阅总量的数据
            foreach (var item in groupTypeData)
            {
                var dateInTypeGroup = new List<string>();
                var countInTypeGroup = new List<int>();
                foreach (var item2 in item)
                {
                    dateInTypeGroup.Add(item2.Date);
                    countInTypeGroup.Add(item2.Count);
                }
                List<int> countInReturnResult = GetInitialList(30);//初始化线的数据
                for (int i = 0; i < 30; i++)
                {
                    if (dateInTypeGroup.Contains(dates[i]))
                    {
                        int queryResultIndex = dateInTypeGroup.IndexOf(dates[i]);
                        countInReturnResult[i] = countInTypeGroup[queryResultIndex];
                        totalCount[i] += countInReturnResult[i];
                    }
                }
                var type = JsonConvert.SerializeObject(item.Key);
                var value = JsonConvert.SerializeObject(countInReturnResult);
                string json = $"{{\"type\":{type},\"value\":{value}}}";
                values.Add(json);
            }
            string lastJson = $"{{\"type\":{JsonConvert.SerializeObject("总借阅量")},\"value\":{JsonConvert.SerializeObject(totalCount)}}}";
            values.Add(lastJson);//借阅总量的线条数据加入

            var x = JsonConvert.SerializeObject(dates);
            var datas = JsonConvert.SerializeObject(values);
            string jsons = $"{{\"x\":{x},\"values\":{datas}}}";
            return Json(jsons);
        }
        #endregion

        #region 辅助方法
        private bool BorrowExists(int id)
        {
            return _context.Borrows.Any(e => e.Id == id);
        }

        public List<int> GetInitialList(int size)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < size; i++)
            {
                list.Add(0);
            }
            return list;
        }
        public List<Borrow>? GetBorrows()
        {
            return _context.Borrows.Include(m => m.Book).AsNoTracking().ToList();
        }

        public DateTime GetTodayLastTime()
        {
            var tomorrow = DateTime.Now.AddDays(1);
            string tomorrowString = tomorrow.Year + "-" + tomorrow.Month + "-" + tomorrow.Day;
            var todayLastTime = DateTime.Parse(tomorrowString);
            return todayLastTime;
        }

        public List<string>? GetDates(DateTime startDate, int daysLength)
        {
            var dates = new List<string>();
            for (int i = 0; i < daysLength; i++)
            {
                var date = startDate.AddDays(i);
                dates.Add(date.ToString("d"));
            }
            return dates;
        }

        public List<NoTypeGroup> GetGroupDataWithNoType(List<Borrow>? borrows, DateTime startDate, DateTime todayLastTime)
        {
            var queryResult = from b in borrows
                              where b.BorrowDate >= startDate && b.BorrowDate <= todayLastTime
                              group b by new {  b.BorrowDate.Year, b.BorrowDate.Month, b.BorrowDate.Day };
            var groupdata = new List<NoTypeGroup>();
            foreach (var item in queryResult)//打印一下按日期和类型一起分组得到的数据
            {
                var date = DateTime.Parse(item.Key.Year + "-" + item.Key.Month + "-" + item.Key.Day);
                groupdata.Add(new NoTypeGroup { Date = date.ToString("d"), Count = item.Count() });
            }
            return groupdata;
        }

        public List<Group> GetGroupData(List<Borrow>? borrows, DateTime startDate, DateTime todayLastTime)
        {
            var queryResult = from b in borrows
                              where b.BorrowDate >= startDate && b.BorrowDate <= todayLastTime
                              group b by new { b.Book.Type, b.BorrowDate.Year, b.BorrowDate.Month, b.BorrowDate.Day };
            var groupdata = new List<Group>();
            foreach (var item in queryResult)//打印一下按日期和类型一起分组得到的数据
            {
                var date = DateTime.Parse(item.Key.Year + "-" + item.Key.Month + "-" + item.Key.Day);
                groupdata.Add(new Group { Type = item.Key.Type, Date = date.ToString("d"), Count = item.Count() });
            }
            return groupdata;
        }

        public IEnumerable<IGrouping<string, Group>>? GetGroupTypeData(List<Group> groupData)
        {
            var groupTypeData = from item in groupData
                                group item by item.Type.ToString();
            return groupTypeData;
        }
        #endregion
    }
}
