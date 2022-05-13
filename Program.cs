using BookMS.Data;
using BookMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BookMS;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("BookContextConnection");
builder.Services.AddDbContext<BookContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true; // Ҫ���������
    options.Password.RequireLowercase = true; // Ҫ�����Сд��ĸ
    options.Password.RequireNonAlphanumeric = false; // Ҫ����������ַ�
    options.Password.RequireUppercase = false; // Ҫ�������д��ĸ
    options.Password.RequiredLength = 6; // ������С���ȣ�Ĭ��Ϊ6
    options.Password.RequiredUniqueChars = 1; // Ψһ�ַ�����Ŀ
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;//��Ҫ�ʼ�ȷ��
}).AddEntityFrameworkStores<BookContext>().AddDefaultTokenProviders();

builder.Services.AddDataProtection();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<BookContext>();
    DbInitializer.Initialize(context);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();//���������֤

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
