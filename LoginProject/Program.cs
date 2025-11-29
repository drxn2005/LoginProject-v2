using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetworkCafesControllers.Data;
using NetworkCafesControllers.Models.Entities;
using NetworkCafesControllers.Models.Settings;
using NetworkCafesControllers.Services.Implementations;
using NetworkCafesControllers.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.SignIn.RequireConfirmedEmail = true;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


#region login extrenal
var config = builder.Configuration;

builder.Services
    .AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = config["Authentication:Google:ClientId"]!;
        options.ClientSecret = config["Authentication:Google:ClientSecret"]!;
    })
    .AddFacebook(options =>
    {
        options.AppId = config["Authentication:Facebook:AppId"]!;
        options.AppSecret = config["Authentication:Facebook:AppSecret"]!;
    })
    .AddTwitter(options =>
    {
        options.ConsumerKey = config["Authentication:Twitter:ConsumerKey"]!;
        options.ConsumerSecret = config["Authentication:Twitter:ConsumerSecret"]!;
        // لو تويتر قافل الإيميل ممكن تحتاج تطلب email claim في الإعدادات
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = config["Authentication:Microsoft:ClientId"]!;
        options.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
    });
#endregion

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// DI
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

var app = builder.Build();

// apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed roles (ADMIN, USER)
    await IdentitySeed.SeedAsync(services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
