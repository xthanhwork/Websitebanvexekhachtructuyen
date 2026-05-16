using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DOANCOSO26.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Mobile")]
        public string? Mobile { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Website")]
        public string? WebsiteUrl { get; set; }

        [Display(Name = "GitHub")]
        public string? GitHubUrl { get; set; }

        [Display(Name = "Twitter")]
        public string? TwitterUrl { get; set; }

        [Display(Name = "Instagram")]
        public string? InstagramUrl { get; set; }

        [Display(Name = "Facebook")]
        public string? FacebookUrl { get; set; }

        [Display(Name = "Ảnh đại diện URL")]
        public string? AvatarUrl { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Roles { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }

    public class UserManagementIndexViewModel
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<UserListItemViewModel> Users { get; set; } = new();
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quyền")]
        [Display(Name = "Quyền")]
        public string Role { get; set; } = Roles.Role_Customer;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quyền")]
        [Display(Name = "Quyền")]
        public string Role { get; set; } = Roles.Role_Customer;

        public bool IsLocked { get; set; }
        public List<string> AvailableRoles { get; set; } = new();
    }

    public class ChatThreadViewModel
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ChatAdminViewModel
    {
        public string? SelectedCustomerId { get; set; }
        public string? SelectedCustomerName { get; set; }
        public List<ChatThreadViewModel> Threads { get; set; } = new();
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
