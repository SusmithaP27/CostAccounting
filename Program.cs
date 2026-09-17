
using CostAccounting.Data;
using CostAccounting.Services;
using CostAccounting.Services.AccountService;
using CostAccounting.Services.EmployeeService;
using CostAccounting.Services.EquipmentRateService;
using CostAccounting.Services.EquipmentService;
using CostAccounting.Services.MaterialRateService;
using CostAccounting.Services.MaterialService;
using CostAccounting.Services.OperationService;
using CostAccounting.Services.UserService;
using CostAccounting.Services.PasswordHasher;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using CostAccounting.Services.EmployeeRateService;
var builder = WebApplication.CreateBuilder(args);

var connectionsStrings = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionsStrings));


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IRoadService, RoadService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IMaterialRateService, MaterialRateService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IEquipmentRateService, EquipmentRateService>();
builder.Services.AddScoped<IOperationService, OperationService>();
// builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAllowanceService, AllowanceService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeRateService, EmployeeRateService>();

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User_Account/Login";
        options.LogoutPath = "/User_Account/Logout";
        options.AccessDeniedPath = "/User_Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        // options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
        // options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // was: Always
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();


//app.MapGet("/", () => "Hello World!");

//app.Run();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

