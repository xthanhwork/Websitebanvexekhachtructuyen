#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using DOANCOSO26.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;

namespace DOANCOSO26.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
            [StringLength(100, ErrorMessage = "Họ tên tối đa {1} ký tự.")]
            [Display(Name = "Họ tên")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            public string? Role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            await EnsureDefaultRolesAsync();
            BindRoleList();

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            await EnsureDefaultRolesAsync();
            BindRoleList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var email = Input.Email?.Trim();
            var user = CreateUser();
            user.FullName = Input.FullName?.Trim();
            user.PhoneNumber = Input.PhoneNumber?.Trim();

            await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Trang đăng ký phía User chỉ cho tạo tài khoản khách hàng.
                // Không lấy Role từ form ẩn để tránh người dùng tự sửa thành Admin/Driver.
                await _userManager.AddToRoleAsync(user, Roles.Role_Customer);

                // Gửi mail xác nhận nếu SMTP chạy được. Nếu SMTP lỗi thì vẫn cho đăng ký thành công,
                // vì hệ thống không bắt buộc xác nhận email mới được đăng nhập.
                try
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code, returnUrl },
                        protocol: Request.Scheme);

                    if (!string.IsNullOrWhiteSpace(callbackUrl))
                    {
                        await _emailSender.SendEmailAsync(email, "Xác nhận tài khoản BusBus",
                            $"Xin chào {HtmlEncoder.Default.Encode(user.FullName ?? email)},<br/>" +
                            $"Vui lòng xác nhận tài khoản bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>bấm vào đây</a>.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không gửi được email xác nhận đăng ký cho {Email}. Tài khoản vẫn được tạo thành công.", email);
                    TempData["Message"] = "Đăng ký thành công. Email xác nhận chưa gửi được do cấu hình SMTP, nhưng bạn vẫn có thể đăng nhập.";
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, TranslateIdentityError(error.Description));
            }

            return Page();
        }

        private async Task EnsureDefaultRolesAsync()
        {
            var roles = new[] { Roles.Role_Customer, Roles.Role_Admin, Roles.Role_Driver };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private void BindRoleList()
        {
            Input ??= new InputModel();
            Input.RoleList = _roleManager.Roles
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
                .ToList();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Không thể tạo instance của '{nameof(ApplicationUser)}'. Hãy kiểm tra class ApplicationUser có constructor rỗng không.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Identity yêu cầu user store có hỗ trợ email.");
            }

            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        private static string TranslateIdentityError(string description)
        {
            if (description.Contains("Passwords must be at least", StringComparison.OrdinalIgnoreCase))
                return "Mật khẩu phải có ít nhất 6 ký tự.";

            if (description.Contains("Passwords must have at least one non alphanumeric", StringComparison.OrdinalIgnoreCase))
                return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt.";

            if (description.Contains("Passwords must have at least one digit", StringComparison.OrdinalIgnoreCase))
                return "Mật khẩu phải có ít nhất 1 chữ số.";

            if (description.Contains("Passwords must have at least one uppercase", StringComparison.OrdinalIgnoreCase))
                return "Mật khẩu phải có ít nhất 1 chữ in hoa.";

            if (description.Contains("Passwords must have at least one lowercase", StringComparison.OrdinalIgnoreCase))
                return "Mật khẩu phải có ít nhất 1 chữ thường.";

            if (description.Contains("is already taken", StringComparison.OrdinalIgnoreCase))
                return "Email này đã được đăng ký. Vui lòng dùng email khác hoặc đăng nhập.";

            return description;
        }
    }
}
