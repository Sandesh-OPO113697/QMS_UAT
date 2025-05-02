using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using QMS.DataBaseService;
using QMS.Encription;
using QMS.Middleware;
using QMS.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/LogIn/Unauthorized";
        options.AccessDeniedPath = "/LogIn/Unauthorized";
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<DLConnection>();
builder.Services.AddTransient<D_Login>();
builder.Services.AddTransient<Dl_Admin>();
builder.Services.AddTransient<DL_SuperAdmin>();
builder.Services.AddTransient<DL_Encrpt>();
builder.Services.AddTransient<DL_Module>();
builder.Services.AddTransient<DlSampling>();
builder.Services.AddTransient<Dl_formBuilder>();
builder.Services.AddTransient<dl_Monitoring>();
builder.Services.AddTransient<DL_Agent>();
builder.Services.AddTransient<DL_QaManager>();
builder.Services.AddTransient<DL_Operation>();
builder.Services.AddTransient<DL_Hr>();
builder.Services.AddTransient<Dl_UpdateManagement>();
builder.Services.AddTransient<Dl_Coaching>();
builder.Services.AddTransient<dl_Supervisor>();
builder.Services.AddTransient<dl_Calibration>();



builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseGlobalErrorHandling();

//app.Use(async (context, next) =>
//{
//    ValidateJWTTocken(context);
//    await next.Invoke(); 
//});


void ValidateJWTTocken(HttpContext context)
{
    string token = context.Request.Cookies["Token"];
    if (string.IsNullOrEmpty(token))
    {
    }
    else
    {
        JWTHelper.AuthenticationRequest(token, context);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LogIn}/{action=UserLogIn}/{id?}");

app.Run();
