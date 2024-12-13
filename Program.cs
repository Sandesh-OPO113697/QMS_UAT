using Microsoft.Extensions.Configuration;
using QMS.DataBaseService;
using QMS.Encription;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout duration as needed
    options.Cookie.HttpOnly = true; // Set cookie to HttpOnly
    options.Cookie.IsEssential = true; // Make session cookie essential
});

// Add IHttpContextAccessor to DI container
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<DLConnection>();
builder.Services.AddScoped<D_Login>();
builder.Services.AddScoped<DL_SuperAdmin>();
builder.Services.AddScoped<DL_Encrpt>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
