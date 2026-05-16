(function () {
    const STORAGE_KEY = "BusBusLang";
    const dict = {
        "Trang chủ": "Home",
        "Tuyến đường": "Routes",
        "Vé của bạn": "Your tickets",
        "Tra cứu vé": "Lookup ticket",
        "Lịch sử đặt vé": "Booking history",
        "Đăng nhập": "Login",
        "Đăng kí": "Register",
        "Đăng ký": "Register",
        "Đăng xuất": "Logout",
        "Tài khoản": "Account",
        "Account": "Account",
        "Xem thông tin": "Profile",
        "Tài khoản cá nhân": "Profile",
        "Chat hỗ trợ": "Support chat",
        "Chat hỗ trợ khách hàng": "Customer support chat",
        "Quản lý tuyến đường": "Route management",
        "Quản lý xe": "Bus management",
        "Quản lý trạm dừng": "Stop management",
        "Quản lý chuyến xe": "Trip management",
        "Quản lý đặt vé": "Booking management",
        "Quản lý tài khoản": "Account management",
        "Quản lý nhân viên – khách hàng": "Staff - customer management",
        "Quản lý nhân viên - khách hàng": "Staff - customer management",
        "Quản lý phân quyền": "Role management",
        "Tìm kiếm": "Search",
        "Lọc": "Filter",
        "Xóa lọc": "Clear filter",
        "Tạo tài khoản": "Create account",
        "Họ tên": "Full name",
        "Số điện thoại": "Phone number",
        "Quyền": "Role",
        "Trạng thái": "Status",
        "Thao tác": "Actions",
        "Đang hoạt động": "Active",
        "Đã khóa": "Locked",
        "Khóa": "Lock",
        "Mở khóa": "Unlock",
        "Sửa / phân quyền": "Edit / assign role",
        "Lưu thay đổi": "Save changes",
        "Đổi mật khẩu": "Change password",
        "Quay lại": "Back",
        "Gửi": "Send",
        "Nhập tin nhắn cần hỗ trợ...": "Type your support message...",
        "Nhập phản hồi cho khách hàng...": "Type your reply to customer..."
    };

    const reverseDict = Object.fromEntries(Object.entries(dict).map(([vi, en]) => [en, vi]));

    function translateTextNode(node, lang) {
        const raw = node.nodeValue;
        if (!raw || !raw.trim()) return;
        const trimmed = raw.trim();
        const translated = lang === "en" ? dict[trimmed] : reverseDict[trimmed];
        if (translated) {
            node.nodeValue = raw.replace(trimmed, translated);
        }
    }

    function translatePlaceholders(lang) {
        document.querySelectorAll("input[placeholder], textarea[placeholder]").forEach(el => {
            const value = el.getAttribute("placeholder");
            if (!value) return;
            const translated = lang === "en" ? dict[value] : reverseDict[value];
            if (translated) el.setAttribute("placeholder", translated);
        });
    }

    function applyLanguage(lang) {
        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, {
            acceptNode: function (node) {
                if (!node.parentElement) return NodeFilter.FILTER_REJECT;
                const tag = node.parentElement.tagName.toLowerCase();
                if (["script", "style", "textarea"].includes(tag)) return NodeFilter.FILTER_REJECT;
                return NodeFilter.FILTER_ACCEPT;
            }
        });

        const nodes = [];
        while (walker.nextNode()) nodes.push(walker.currentNode);
        nodes.forEach(node => translateTextNode(node, lang));
        translatePlaceholders(lang);
        document.documentElement.lang = lang === "en" ? "en" : "vi";
    }

    function renderSwitcher() {
        if (document.getElementById("busbus-language-switcher")) return;
        const box = document.createElement("div");
        box.id = "busbus-language-switcher";
        box.style.position = "fixed";
        box.style.right = "18px";
        box.style.bottom = "18px";
        box.style.zIndex = "999999";
        box.style.background = "#03045e";
        box.style.borderRadius = "999px";
        box.style.boxShadow = "0 8px 24px rgba(0,0,0,.18)";
        box.style.overflow = "hidden";
        box.innerHTML = `
            <button type="button" data-lang="vi" style="border:0;padding:8px 12px;background:transparent;color:white;font-weight:700;">VI</button>
            <button type="button" data-lang="en" style="border:0;padding:8px 12px;background:transparent;color:white;font-weight:700;">EN</button>`;
        document.body.appendChild(box);

        box.addEventListener("click", function (e) {
            const button = e.target.closest("button[data-lang]");
            if (!button) return;
            const lang = button.getAttribute("data-lang");
            localStorage.setItem(STORAGE_KEY, lang);
            location.reload();
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        renderSwitcher();
        const lang = localStorage.getItem(STORAGE_KEY) || "vi";
        if (lang === "en") applyLanguage("en");
    });
})();
