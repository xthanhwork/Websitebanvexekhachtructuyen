using DOANCOSO26.Data;
using DOANCOSO26.Hubs;
using DOANCOSO26.Models;
using DOANCOSO26.Repository;
using DOANCOSO26.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Thiết lập mật khẩu dễ dùng hơn cho đồ án/demo.
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// SignalR
builder.Services.AddSignalR()
    .AddHubOptions<ChatHub>(options =>
    {
        options.EnableDetailedErrors = true;
    });

// Repositories
builder.Services.AddScoped<IBusRepository, EFBusRepository>();
builder.Services.AddScoped<IBusTripRepository, EFBusTripRepository>();
builder.Services.AddScoped<ISeatRepository, EFSeatRepository>();
builder.Services.AddScoped<IStopRepository, EFStopRepository>();
builder.Services.AddScoped<IBookingRepository, EFBookingRepository>();
builder.Services.AddScoped<IBusRouteRepository, EFBusRouteRepository>();
builder.Services.AddScoped<ITripReportRepository, EFTripReportRepository>();
builder.Services.AddScoped<IDriverregisRepository, EFDriverregisRepository>();

// Services
builder.Services.AddScoped<GoogleMapService>();
builder.Services.AddScoped<IVnPaySevices, VnPaySevices>();
builder.Services.AddScoped<ChatService>();

// Email service
// ReservationController đang inject trực tiếp SmtpEmailSender,
// còn Identity có thể dùng IEmailSender.
builder.Services.AddScoped<SmtpEmailSender>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// Helpers
builder.Services.AddSingleton<InvoiceCodeGenerator>();

var app = builder.Build();

// Tạo sẵn role và bảng ChatMessages nếu database cũ chưa có.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { Roles.Role_Admin, Roles.Role_Customer, Roles.Role_Driver })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[dbo].[ChatMessages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChatMessages](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ChatMessages] PRIMARY KEY,
        [ConversationId] NVARCHAR(450) NOT NULL,
        [SenderId] NVARCHAR(450) NOT NULL,
        [SenderName] NVARCHAR(256) NOT NULL,
        [SenderRole] NVARCHAR(50) NOT NULL,
        [MessageText] NVARCHAR(MAX) NOT NULL,
        [SentAt] DATETIME2 NOT NULL CONSTRAINT [DF_ChatMessages_SentAt] DEFAULT GETDATE(),
        [IsRead] BIT NOT NULL CONSTRAINT [DF_ChatMessages_IsRead] DEFAULT 0
    );
    CREATE INDEX [IX_ChatMessages_ConversationId] ON [dbo].[ChatMessages]([ConversationId]);
END");
}


// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Bus}/{action=Dash}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();