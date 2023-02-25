using RazorWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<MyBlogContext>(options =>
{
  string connectString = builder.Configuration.GetConnectionString("MyBlogContext");
  options.UseSqlServer(connectString);
});

builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<MyBlogContext>()
                .AddDefaultTokenProviders();

// builder.Services.AddDefaultIdentity<AppUser>()
//                 .AddEntityFrameworkStores<MyBlogContext>();

builder.Services.AddOptions();
var mailSettings = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailSettings);
builder.Services.AddSingleton<IEmailSender, SendMailService>();

// Truy cập IdentityOptions
builder.Services.Configure<IdentityOptions>(options =>
{
  // Thiết lập về Password
  options.Password.RequireDigit = false; // Không bắt phải có số
  options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
  options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
  options.Password.RequireUppercase = false; // Không bắt buộc chữ in
  options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
  options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

  // Cấu hình Lockout - khóa user
  options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
  options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
  options.Lockout.AllowedForNewUsers = true;

  // Cấu hình về User.
  options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
      "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
  options.User.RequireUniqueEmail = true;  // Email là duy nhất

  // Cấu hình đăng nhập.
  options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
  options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
  options.SignIn.RequireConfirmedAccount = true;

});

builder.Services.ConfigureApplicationCookie(options =>
{
  options.LoginPath = "/login/";
  options.LogoutPath = "/logout/";
  options.AccessDeniedPath = "/khongduoctruycap.html";
});

builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                  var gconfig = builder.Configuration.GetSection("Authentication:Google");
                  options.ClientId = gconfig["ClientId"];
                  options.ClientSecret = gconfig["ClientSecret"];

                  // https://localhost:5221/signin-google
                  options.CallbackPath = "/dang-nhap-tu-google";
                })
                .AddFacebook(options =>
                {
                  var fconfig = builder.Configuration.GetSection("Authentication:Facebook");
                  options.AppId = fconfig["AppId"];
                  options.AppSecret = fconfig["AppSecret"];
                  options.CallbackPath = "/dang-nhap-tu-facebook";
                });

var app = builder.Build();

app.UseCookiePolicy(new CookiePolicyOptions()
{
  MinimumSameSitePolicy = SameSiteMode.Lax
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
