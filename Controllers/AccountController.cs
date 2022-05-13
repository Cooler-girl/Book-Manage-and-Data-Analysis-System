using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BookMS.Data;
using BookMS.Models;
using BookMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace BookMS.Controllers
{
    public class AccountController : Controller
    {
        #region 依赖注入
        //UserManager 管理用户的添加、修改、查找
        //UserManager.FindByNameAsync() 根据用户账号查找用户

        //SignInManager 用户的登入登出操作类
        //SignInManager.PasswordSignInAsync() 根据用户信息和密码判断是否正确，且登入
        //SignInManager.SignOutAsync() 登出
        //SignInManager.IsSignedIn(User) 判断是否是登入状态
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        private readonly BookContext _context;

        //private readonly IEmailSender _sender;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<ChangePasswordModel> logger, BookContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            //_sender = sender;
        }
        #endregion

        #region 注册模块
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (model.PassWord == null||model.Email==null||model.Name==null||model.UserName==null)
            {
                return View();
            }
            //ModelState.Clear();
            Console.WriteLine("用户名：{0}\n密码：{1}\n邮箱：{2}\n昵称：{3}", model.UserName, model.PassWord, model.Email, model.Name);
            if (!ModelState.IsValid)
            {
                Console.WriteLine("模型无效！");
                List<string> sb = new List<string>();
                //获取所有错误的Key
                List<string> Keys = ModelState.Keys.ToList();
                //获取每一个key对应的ModelStateDictionary
                foreach (var key in Keys)
                {
                    var errors = ModelState[key].Errors.ToList();
                    //将错误描述添加到sb中
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                Console.WriteLine("错误信息打印结束");
                return View();
            }

            if (_context.Users.FirstOrDefault(m => m.Email == model.Email) != null)
            {
                ModelState.AddModelError(string.Empty, "该邮箱已被使用！");
                return View();
            }

            // UserManager 添加用户的方法，注意密码是做为第二个参数传递的。数据库中的表是没有PassWord字段，而是一个加密的字段PasswordHash
            User user = new User { UserName = model.UserName, Email = model.Email, Name = model.Name };

            var result = await _userManager.CreateAsync(user, model.PassWord);

            if (result.Succeeded)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { Controller = "Account", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                /*await _sender.SendEmailAsync(model.Email, "确认邮件",
                        $"请通过点击 <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>这里来确认您的账户</a>.");*/
                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    Console.WriteLine("需要验证邮件！");
                    return RedirectToAction("RegisterConfirm", new { email = model.Email, returnUrl = returnUrl });
                }
                else
                {
                    Console.WriteLine("不需要验证邮件。");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> RegisterConfirm(string email, string returnUrl = null)
        {
            RegisterConfirmationModel model = new RegisterConfirmationModel();
            Console.WriteLine("邮箱：{0}\n返回地址：{1}", email, returnUrl);
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            model.Email = email;
            // Once you add a real email sender, you should remove this code that lets you confirm the account
            model.DisplayConfirmAccountLink = true;
            if (model.DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                model.EmailConfirmationUrl = "/Account/ConfirmEmail?userId=" + userId + "&code=" + code;
            }
            Console.WriteLine("确认连接的地址：{0}", model.EmailConfirmationUrl);
            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            ConfirmEmailModel model = new ConfirmEmailModel();
            if (userId == null || code == null)
            {
                return RedirectToAction("/Index");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                model.StatusMessage = "感谢您确认账户.";
                return RedirectToAction(nameof(Login));
            }
            else
            {
                model.StatusMessage = "确认邮箱时发生错误.";
                return View(model);
            }

        }
        #endregion

        #region 登录模块
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("模型无效！");
                ModelState.AddModelError(string.Empty, "登录异常！");
                return View();
            }
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                Console.WriteLine("用户不存在！");
                ModelState.AddModelError(string.Empty, "用户不存在！");
                return View();
            }
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "用户未确认！");
                return View();
            }
            // 后面的两个参数，一个是是否应用到浏览器Cookie，一个是登录失败是否锁定账户。
            var result = await _signInManager.PasswordSignInAsync(user, model.PassWord, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(String.Empty, "账户或密码不正确");
            return View(model); ;
        }
        #endregion

        #region 忘记密码模块
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgotPasswordModel model, string returnUrl = null)
        {
            Console.WriteLine("\nForget\n");
            returnUrl ??= Url.Content("~/");
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    Console.WriteLine("不存在该账户或该账户未确认！");
                    ModelState.AddModelError(String.Empty, "不存在该账户或该账户未确认！");
                    return View();
                }
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                //Console.WriteLine("\n\nForgetPassword函数里生成的token：{0}\n\n",code);
                //var callbackUrl = "/Account/ResetPassword?code=" + code;
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code },
                    protocol: Request.Scheme);
                return RedirectToAction("ForgotPasswordConfirmation", new { email = model.Email, code, returnUrl = returnUrl });
                //return RedirectToAction("ResetPassword", new {  code });
            }
            return View(model);
        }

        public async Task<IActionResult> ForgotPasswordConfirmation(string email, string code, string returnUrl = null)
        {
            ForgotPasswordConfirmation model = new ForgotPasswordConfirmation();
            if (email == null)
            {
                Console.WriteLine("邮件地址为空!");
                ModelState.AddModelError(string.Empty, "邮件地址为空！");
                return RedirectToAction(nameof(ForgetPassword));
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                Console.WriteLine("用户不存在！");
                ModelState.AddModelError(string.Empty, "用户不存在！");
                return RedirectToAction(nameof(ForgetPassword));
            }
            model.Email = email;
            model.DisplayConfirmAccountLink = true;
            if (model.DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                model.EmailConfirmationUrl = "/Account/ResetPassword?code=" + Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))+"&email="+model.Email;/*
                var url = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new {  userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
                model.EmailConfirmationUrl = url;*/
                Console.WriteLine("确认忘记里的连接地址：{0}", model.EmailConfirmationUrl);
            }

            Console.WriteLine("\n\nForgetPasswordConfirmation里的token：{0}\n\n", code);
            return View(model);
        }

        public IActionResult ResetPassword(string email,string code = null)
        {
            if (code == null)
            {
                ModelState.AddModelError(string.Empty, "token为空！");
                return RedirectToAction(nameof(ForgetPassword));
            }
            else
            {
                //Console.WriteLine("\n\nResetPassword(GET)里的token：{0}\n\n", code);
                ResetPasswordModel model = new ResetPasswordModel { Code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)) };
                model.Email=email;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            //Console.WriteLine("邮箱：{0}\n密码：{1}\n确认密码：{2}\nCode：{3}",model.Email,model.Password,model.ConfirmPassword,model.Code);
            ModelState.Clear();
            if (!ModelState.IsValid)
            {
                Console.Write("模型无效！");
                ModelState.AddModelError(string.Empty, "输入无效！");
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            /*
            if(user != null)
            {
                var token=await  _userManager.GeneratePasswordResetTokenAsync(user);
                var res=await _userManager.ResetPasswordAsync(user,token,model.Password);
                if (res.Succeeded)
                {
                    Console.WriteLine("测试成功！");
                }
                else
                {
                    Console.WriteLine("测试失败！");
                }
                return View();
            }*/

            if (user == null)
            {
                Console.WriteLine("用户不存在！");
                ModelState.AddModelError(string.Empty, "用户不存在！");
                return View(model);
            }
            //Console.WriteLine("\n\nResetPasswordAsync函数执行前的token：{0}\n\n",model.Code);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
            //var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                Console.WriteLine("重置成功！跳转到确认界面。。。");
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        #endregion

        #region 退出登录模块
        [Authorize]
        //登出，并且跳转到登录页面
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();//登出
            return RedirectToAction(nameof(Login));//返回登录页
        }
        #endregion

        #region 拒绝访问页面
        [Authorize]
        public IActionResult AccessDenied(string ReturnUrl)
        {
            if (ReturnUrl != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        #endregion

        #region 修改密码模块
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);//获取已登录的用户
            if (user == null)
            {
                return NotFound($"不能通过ID加载用户数据'{_userManager.GetUserId(User)}'.");
            }

            /*var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }*/
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            ModelState.Clear();
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"不能通过ID加载用户数据 '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.Input.OldPassword, model.Input.NewPassword);
            if (!changePasswordResult.Succeeded)//修改失败
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("用户成功修改密码！");
            model.StatusMessage = "您的密码已经改变.";
            return View();
        }
        #endregion

        #region 注销模块
        [Authorize]
        public async Task<IActionResult> LogOff()
        {
            LogOffModel model = new LogOffModel();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"不能通过ID加载用户数据'{_userManager.GetUserId(User)}'.");
            }

            model.RequirePassword = await _userManager.HasPasswordAsync(user);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LogOff(LogOffModel model)
        {
            ModelState.Clear();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"不能通过ID加载用户数据 '{_userManager.GetUserId(User)}'.");
            }

            model.RequirePassword = await _userManager.HasPasswordAsync(user);
            if (model.RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, model.Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "密码错误.");
                    return View(model);
                }
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var borrow = _context.Borrows.FirstOrDefault(m => (m.UserID == userId) && (m.state == "借阅中"));

            if (borrow != null)//有借阅中的图书
            {
                ModelState.AddModelError(string.Empty, "您有借阅中的图书未归还，请归还后再试！");
                return View(model);
            }

            if (await _userManager.IsInRoleAsync(user, "超级管理员"))
            {
                ModelState.AddModelError(string.Empty, "您是尊贵的超级管理员，责任重于泰山，不准你注销！");
                //await Response.WriteAsync("<script>alert('不准!');window.location.href ='Edit'</script>");
                return View(model);
            }

             var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"删除用户时发生了未预料的错误！.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("ID为 '{UserId}'的用户已被删除.", userId);

            return RedirectToAction("Index", "Home");

        }
        #endregion

        #region 重发确认邮件
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationModel model, string returnUrl = null)
        {
            ModelState.Clear();

            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(model.Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "未找到用户，请修改后再试！");
                return View();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme);

            ModelState.AddModelError(string.Empty, "确认邮件已发送，请检查您的邮箱.");
            return RedirectToAction("RegisterConfirm", new { email = model.Input.Email, returnUrl = returnUrl });
        }
        #endregion

        #region 修改资料
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);//获取当前登录的用户

            if (user == null)
            {
                return NotFound($"不能通过ID加载用户数据 '{_userManager.GetUserId(User)}'.");
            }

            /*var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var email=await _userManager.GetEmailAsync(user);
            var name="";
            if (User.Identity != null)
            {
                name = User.Identity.Name;
            }
            Console.WriteLine("昵称：{0}", name);*/
            EditModel model = new EditModel
            {
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(EditModel model)
        {
            ModelState.Clear();

            if (!ModelState.IsValid)
            {
                Console.WriteLine("模型无效！");
                ModelState.AddModelError(string.Empty, "数据异常！");
                return View();
            }
            User currentUser =await _userManager.GetUserAsync(HttpContext.User); 
            if (model.Email!=currentUser.Email&&_context.Users.FirstOrDefault(m => m.Email == model.Email) != null) 
            {
                ModelState.AddModelError(string.Empty, "该邮箱已被使用！");
                return View();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"不能通过ID加载用户数据 '{_userManager.GetUserId(User)}'.");
            }
            user.Name = model.Name;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                ModelState.AddModelError(String.Empty,"您的资料已更新！");
                return View(model);
            }
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }

            await _signInManager.RefreshSignInAsync(user);
            model.StatusMessage = "您的资料已经更新";
            return View(model);
        }

        #endregion

        #region 用户管理
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
            //var users= await _userManager.Users.AsNoTracking().ToListAsync();
            var users = from u in _context.Users
                        select u;
            int pageSize = 5;

            if (!String.IsNullOrEmpty(searchString))
            {
                users=users.Where(u =>u.Name.Contains(searchString)||u.Email.Contains(searchString));
            }
                return View(await PaginatedList<User>.CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        #endregion

        #region 管理员修改用户资料
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> Modify(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modify(string id, [Bind("Id,Name,Email,PhoneNumber")] User u)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("模型无效！");
                ModelState.AddModelError(string.Empty, "数据异常！");
                return View();
            }
            User currentUser = await _userManager.GetUserAsync(User);
            
            var user = await _userManager.FindByIdAsync(id);

            if (_context.Users.FirstOrDefault(m => m.Email == u.Email) != null)
            {
                if (u.Email != user.Email && u.Email != currentUser.Email)
                {
                    ModelState.AddModelError(string.Empty, "此邮箱已被使用！");
                    return View(u);
                }
            }

            if (user == null)
            {
                return NotFound($"不能通过ID加载用户数据 '{_userManager.GetUserId(User)}'.");
            }
            if (u.Name != null)
            {
                user.Name = u.Name;
            }
            if (u.Email != null)
            {
                user.Email = u.Email;
            }
            if (u.PhoneNumber != null)
            {
                user.PhoneNumber = u.PhoneNumber;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "您的资料已经更新");
                return RedirectToAction(nameof(Manage));
            }
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }
            return View(user);
        }
        #endregion

        #region 管理员删除用户
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (await _userManager.IsInRoleAsync(user, "管理员"))
            {
                ModelState.AddModelError(string.Empty, "不允许删除管理员！");
                return RedirectToAction(nameof(Manage));

            }

            var borrow = _context.Borrows.FirstOrDefault(m => (m.UserID == id) && (m.state == "借阅中"));
            if (borrow != null)//有借阅中的图书
            {
                ModelState.AddModelError(string.Empty, "该用户有借阅中的图书未归还，请通知其处理归还后再试！");
                return RedirectToAction(nameof(Manage));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"删除用户时发生了未预料的错误！.");
            }
            _logger.LogInformation("ID为 '{UserId}'的用户已被删除.", id);
            return RedirectToAction(nameof(Manage));
        }
        #endregion
    }
}
