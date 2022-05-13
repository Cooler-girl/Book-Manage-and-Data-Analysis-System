using BookMS.Models;
using BookMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookMS.Controllers
{
    public class RoleController : Controller
    {
        #region 依赖注入
        //RoleManager<IdentityRole> roleManager;
        //roleManager.FindByIdAsync() 根据Id查询角色。
        //roleManager.CreateAsync() 创建角色。
        //roleManager.UpdateAsync() 更新角色。

        // 这里没有像用户管理那样扩展了IdentityUser,这里使用了基础的IdentityRole
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        #endregion

        #region 角色列表
        public async Task<IActionResult> Index()
        {
            return View(await _roleManager.Roles.ToListAsync());
        }
        #endregion

        #region 创建角色
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(RoleViewModel model)
        {
            ModelState.Clear();
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "数据异常");
                return View(model);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole
            {
                Name = model.Name
            });
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }


            return View(model);
        }
        #endregion

        #region 编辑角色
        public async Task<IActionResult> Edit(string Id)
        {
            var model = await _roleManager.FindByIdAsync(Id);
            // 获取角色的所有用户
            var users = await _userManager.GetUsersInRoleAsync(model.Name);
            ViewData["users"] = users;

            return View(nameof(Edit), new RoleViewModel
            {
                Id = model.Id,
                Name = model.Name
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RoleViewModel model)
        {
            ModelState.Clear();
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "数据异常");
                return View(model);
            }

            var mod = await _roleManager.FindByIdAsync(model.Id);
            if (mod == null)
            {
                ModelState.AddModelError(string.Empty, "角色不存在！");
            }
            mod.Name = model.Name;
            var result = await _roleManager.UpdateAsync(mod);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        #endregion

        #region 删除角色
        public async Task<IActionResult> Delete(string id)
        {
            var model=await _roleManager.FindByIdAsync(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(IdentityRole model)
        {
            var mod= await _roleManager.FindByIdAsync(model.Id);
            var result = await _roleManager.DeleteAsync(mod);
            if (result.Succeeded)
            {
                Console.WriteLine("删除角色成功。");
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("删除角色失败！");
            ModelState.AddModelError(string.Empty, "删除角色时发生未预料的错误！");
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region 给用户分配角色
        //给用户分配角色
        //roleManager.FindByIdAsync(Id) 获取角色。
        //userManager.Users.ToListAsync() 获取所有用户，用于分配角色。
        //userManager.IsInRoleAsync(user, role.Name) 判断用户是否已分配角色。
        //userManager.FIndByIdAsync(userId) 获取用户。
        //userManager.AddToRoleAsync(user, role.Name) 分配角色给用户。       
        public async Task<IActionResult> AddRoleToUser(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return RedirectToAction(nameof(Index));
            }
            var model = new UserRoleViewModel
            {
                RoleId = roleId
            };
            var allUser = await _userManager.Users.ToListAsync();
            foreach (var u in allUser)
            {
                // 判断用户是否已经分配到该角色
                if (!await _userManager.IsInRoleAsync(u, role.Name))
                {
                    model.Users.Add(u);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddRoleToUser(UserRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "数据异常！");
                return View(model);
            }
            var user = await _userManager.FindByIdAsync(model.UserId);
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (user == null || role == null)
            {
                ModelState.AddModelError(string.Empty, "用户不存在或者角色不存在！");
                return View();
            }
            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (result.Succeeded)
            {
                // 添加成功跳转到角色编辑页面
                return RedirectToAction(nameof(Edit), new { Id = model.RoleId });                
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        #endregion

        #region 取消用户角色        
        public async Task<IActionResult> CancelRoleForUser(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return RedirectToAction(nameof(Index));
            }
            var model = new UserRoleViewModel
            {
                RoleId = roleId
            };
            var allUser = await _userManager.Users.ToListAsync();
            foreach (var u in allUser)
            {
                // 判断用户是否已经分配到该角色
                if (await _userManager.IsInRoleAsync(u, role.Name))
                {
                    model.Users.Add(u);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CancelRoleForUser(UserRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "数据异常！");
                return View(model);
            }
            var user = await _userManager.FindByIdAsync(model.UserId);
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (user == null || role == null)
            {
                ModelState.AddModelError(string.Empty, "用户不存在或者角色不存在！");
                return View();
            }
            var result = await _userManager.RemoveFromRoleAsync(user,role.Name);
            if (result.Succeeded)
            {
                // 取消角色成功跳转到角色编辑页面
                return RedirectToAction(nameof(Edit), new { Id = model.RoleId });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        #endregion

        #region 设置管理员
        [Authorize(Roles = "管理员,超级管理员")]
        public async Task<IActionResult> AddAdminToUser()
        {
            var role = await _roleManager.FindByNameAsync("管理员");
            if (role == null)
            {
                ModelState.AddModelError(String.Empty, "没有管理员这个角色！");
                return NotFound();
            }
            var model = new UserRoleViewModel
            {
                RoleId = role.Id
            };
            var allUser = await _userManager.Users.ToListAsync();
            foreach (var u in allUser)
            {
                // 判断用户是否已经分配到该角色
                if (!await _userManager.IsInRoleAsync(u, role.Name))
                {
                    model.Users.Add(u);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddAdminToUser(UserRoleViewModel model)
        {
            ModelState.Clear();
            if (!ModelState.IsValid)
            {
                //Console.WriteLine("模型无效！");
                ModelState.AddModelError(string.Empty, "数据异常！");
                return View(model);
            }
            var user = await _userManager.FindByIdAsync(model.UserId);
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (user == null || role == null)
            {
                //Console.WriteLine("用户或角色不存在！");
                ModelState.AddModelError(string.Empty, "用户不存在或者角色不存在！");
                return View();
            }
            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (result.Succeeded)
            {
                //Console.WriteLine("设置管理员成功！");
                // 设置管理员成功跳转到设置管理员页面
                ModelState.AddModelError(String.Empty, "设置管理员成功！");
                await AddAdminToUser();
                //return View();
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        #endregion

        #region 取消管理
        [Authorize(Roles = "超级管理员")]
        public async Task<IActionResult> CancelAdminForUser()
        {
            var role = await _roleManager.FindByNameAsync("管理员");
            if (role == null)
            {
                ModelState.AddModelError(String.Empty, "没有管理员这个角色！");
                return NotFound();
            }

            var model = new UserRoleViewModel
            {
                RoleId = role.Id
            };
            var allUser = await _userManager.Users.ToListAsync();
            foreach (var u in allUser)
            {
                // 判断用户是否已经分配到该角色
                if (await _userManager.IsInRoleAsync(u, role.Name))
                {
                    model.Users.Add(u);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CancelAdminForUser(UserRoleViewModel model)
        {
            var roleId = model.RoleId;

            if (!ModelState.IsValid)
            {
                Console.WriteLine("模型无效！");
                ModelState.AddModelError(string.Empty, "数据异常！");
                return View(model);
            }
            var user = await _userManager.FindByIdAsync(model.UserId);
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (user == null || role == null)
            {
                ModelState.AddModelError(string.Empty, "用户不存在或者角色不存在！");
                return View(model);
            }
            if(await _userManager.IsInRoleAsync(user, "超级管理员"))
            {
                ModelState.AddModelError(string.Empty, "超级管理员必须是管理员！");
                return View(model);
            }
            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
            if (result.Succeeded)
            {
                // 取消管理员成功跳转到取消管理员界面
                ModelState.AddModelError(string.Empty, "已成功取消用户的管理员权限！");
                await CancelAdminForUser();
                //return View();
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var mod = new UserRoleViewModel
            {
                RoleId = role.Id
            };
            var allUser = await _userManager.Users.ToListAsync();
            foreach (var u in allUser)
            {
                // 判断用户是否已经分配到该角色
                if (await _userManager.IsInRoleAsync(u, role.Name))
                {
                    mod.Users.Add(u);
                }
            }
            return View(mod);
        }
        #endregion
    }
}
