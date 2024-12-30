using Microsoft.Extensions.Configuration;
using QMS.DataBaseService;
using QMS.Encription;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DLConnection>();
builder.Services.AddScoped<D_Login>();
builder.Services.AddScoped<Dl_Admin>();
builder.Services.AddScoped<DL_SuperAdmin>();
builder.Services.AddScoped<DL_Encrpt>();
builder.Services.AddControllersWithViews();

var app = builder.Build();


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
