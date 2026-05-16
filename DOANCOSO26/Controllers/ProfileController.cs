using DOANCOSO26.Data;
using DOANCOSO26.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace DOANCOSO26.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await EnsureProfileExtraTableAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var model = await LoadProfileModelAsync(user);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            await EnsureProfileExtraTableAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim();
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null && existingUser.Id != user.Id)
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

            await SaveProfileExtraAsync(user.Id, model);
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Cập nhật tài khoản cá nhân thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            IdentityResult result;
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            }
            else
            {
                result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            }

            if (!result.Succeeded)
            {
                AddIdentityErrors(result);
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ProfileViewModel> LoadProfileModelAsync(ApplicationUser user)
        {
            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            };

            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = @"
SELECT TOP 1 Mobile, Address, WebsiteUrl, GitHubUrl, TwitterUrl, InstagramUrl, FacebookUrl, AvatarUrl
FROM dbo.UserProfileExtras
WHERE UserId = @UserId";
                AddParameter(command, "@UserId", user.Id);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    model.Mobile = ReadString(reader, "Mobile");
                    model.Address = ReadString(reader, "Address");
                    model.WebsiteUrl = ReadString(reader, "WebsiteUrl");
                    model.GitHubUrl = ReadString(reader, "GitHubUrl");
                    model.TwitterUrl = ReadString(reader, "TwitterUrl");
                    model.InstagramUrl = ReadString(reader, "InstagramUrl");
                    model.FacebookUrl = ReadString(reader, "FacebookUrl");
                    model.AvatarUrl = ReadString(reader, "AvatarUrl");
                }
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }

            return model;
        }

        private async Task SaveProfileExtraAsync(string userId, ProfileViewModel model)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = @"
IF EXISTS (SELECT 1 FROM dbo.UserProfileExtras WHERE UserId = @UserId)
BEGIN
    UPDATE dbo.UserProfileExtras
    SET Mobile = @Mobile,
        Address = @Address,
        WebsiteUrl = @WebsiteUrl,
        GitHubUrl = @GitHubUrl,
        TwitterUrl = @TwitterUrl,
        InstagramUrl = @InstagramUrl,
        FacebookUrl = @FacebookUrl,
        AvatarUrl = @AvatarUrl,
        UpdatedAt = GETDATE()
    WHERE UserId = @UserId
END
ELSE
BEGIN
    INSERT INTO dbo.UserProfileExtras
        (UserId, Mobile, Address, WebsiteUrl, GitHubUrl, TwitterUrl, InstagramUrl, FacebookUrl, AvatarUrl, UpdatedAt)
    VALUES
        (@UserId, @Mobile, @Address, @WebsiteUrl, @GitHubUrl, @TwitterUrl, @InstagramUrl, @FacebookUrl, @AvatarUrl, GETDATE())
END";

                AddParameter(command, "@UserId", userId);
                AddParameter(command, "@Mobile", Clean(model.Mobile));
                AddParameter(command, "@Address", Clean(model.Address));
                AddParameter(command, "@WebsiteUrl", Clean(model.WebsiteUrl));
                AddParameter(command, "@GitHubUrl", Clean(model.GitHubUrl));
                AddParameter(command, "@TwitterUrl", Clean(model.TwitterUrl));
                AddParameter(command, "@InstagramUrl", Clean(model.InstagramUrl));
                AddParameter(command, "@FacebookUrl", Clean(model.FacebookUrl));
                AddParameter(command, "@AvatarUrl", Clean(model.AvatarUrl));

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private async Task EnsureProfileExtraTableAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[dbo].[UserProfileExtras]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserProfileExtras]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Mobile] NVARCHAR(50) NULL,
        [Address] NVARCHAR(500) NULL,
        [WebsiteUrl] NVARCHAR(500) NULL,
        [GitHubUrl] NVARCHAR(500) NULL,
        [TwitterUrl] NVARCHAR(500) NULL,
        [InstagramUrl] NVARCHAR(500) NULL,
        [FacebookUrl] NVARCHAR(500) NULL,
        [AvatarUrl] NVARCHAR(500) NULL,
        [UpdatedAt] DATETIME NOT NULL CONSTRAINT [DF_UserProfileExtras_UpdatedAt] DEFAULT GETDATE(),
        CONSTRAINT [FK_UserProfileExtras_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_UserProfileExtras_UserId] ON [dbo].[UserProfileExtras]([UserId]);
END");
        }

        private static void AddParameter(DbCommand command, string name, object? value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private static string? ReadString(DbDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
