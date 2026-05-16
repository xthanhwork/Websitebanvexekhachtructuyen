using DOANCOSO26.Models;
using AppRoles = DOANCOSO26.Models.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DOANCOSO26.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? search, string? role)
        {
            await EnsureDefaultRolesAsync();

            var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
            var rows = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim();
                    var matched = (user.FullName ?? string.Empty).Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                  || (user.Email ?? string.Empty).Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                  || (user.PhoneNumber ?? string.Empty).Contains(keyword, StringComparison.OrdinalIgnoreCase);
                    if (!matched)
                    {
                        continue;
                    }
                }

                rows.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Roles = string.Join(", ", roles),
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now
                });
            }

            var model = new UserManagementIndexViewModel
            {
                Search = search,
                Role = role,
                Roles = await _roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToListAsync(),
                Users = rows
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await EnsureDefaultRolesAsync();
            ViewBag.Roles = await GetRolesAsync();
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            await EnsureDefaultRolesAsync();
            ViewBag.Roles = await GetRolesAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingEmail = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (existingEmail != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã tồn tại.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName.Trim(),
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                AddIdentityErrors(result);
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }

            await _userManager.AddToRoleAsync(user, model.Role);
            TempData["SuccessMessage"] = "Tạo tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            await EnsureDefaultRolesAsync();

            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = userRoles.FirstOrDefault() ?? AppRoles.Role_Customer,
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now,
                AvailableRoles = await GetRolesAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            await EnsureDefaultRolesAsync();
            model.AvailableRoles = await GetRolesAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            var email = model.Email.Trim();
            var existingEmail = await _userManager.FindByEmailAsync(email);
            if (existingEmail != null && existingEmail.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng cho tài khoản khác.");
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, email);
                if (!setEmailResult.Succeeded)
                {
                    AddIdentityErrors(setEmailResult);
                    return View(model);
                }

                var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
                if (!setUserNameResult.Succeeded)
                {
                    AddIdentityErrors(setUserNameResult);
                    return View(model);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddIdentityErrors(updateResult);
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await _userManager.AddToRoleAsync(user, model.Role);

            if (model.IsLocked)
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(100));
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            TempData["SuccessMessage"] = "Cập nhật tài khoản và phân quyền thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now;
            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(100));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            await EnsureDefaultRolesAsync();
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            roleName = (roleName ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(roleName) && !await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                TempData["SuccessMessage"] = "Tạo quyền mới thành công.";
            }
            return RedirectToAction(nameof(Roles));
        }

        private async Task<List<string>> GetRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToListAsync();
        }

        private async Task EnsureDefaultRolesAsync()
        {
            var roles = new[] { AppRoles.Role_Admin, AppRoles.Role_Customer, AppRoles.Role_Driver };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
