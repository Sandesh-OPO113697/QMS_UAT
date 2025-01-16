using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using QMS.DataBaseService;
using QMS.Encription;
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
        options.LoginPath = "/LogIn/Unauthorized"; // Redirect to unauthorized page if forbidden
        options.AccessDeniedPath = "/LogIn/Unauthorized"; // Redirect to unauthorized page if forbidden
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DLConnection>();
builder.Services.AddScoped<D_Login>();
builder.Services.AddScoped<Dl_Admin>();
builder.Services.AddScoped<DL_SuperAdmin>();
builder.Services.AddScoped<DL_Encrpt>();
builder.Services.AddScoped<DL_Module>();
builder.Services.AddScoped<DlSampling>();


builder.Services.AddControllersWithViews();

var app = builder.Build();
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
