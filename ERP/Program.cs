//using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ERP
{
    public class Program
    {
        public static async Task Main(string[] args)
        {


            var cultureInfo = new CultureInfo("en-US"); // ������������� �������� � ������
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ErpContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            //builder.Services.AddDefaultIdentity<ErpUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ErpContext>();
            //builder.Services.AddControllersWithViews();

            // ��������� ������ AddIdentity, ����� �������� ���������
            builder.Services.AddIdentity<ErpUser, IdentityRole>()
                .AddEntityFrameworkStores<ErpContext>()
                .AddDefaultTokenProviders(); // ��������� ������ ��� ������ ������ � ������ ������� 


            //builder.Services.ConfigureApplicationCookie(options =>
            //{
            //    options.LoginPath = "/Account/Login"; // ���� � ����� �������� �����
            //    options.LogoutPath = "/Account/Logout";
            //    options.AccessDeniedPath = "/Account/AccessDenied";
            //});


            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "LoginKuk";  // ��� cookie ��� ��������������
                options.Cookie.HttpOnly = true;             // ������ ��� HTTP-��������, �� �������� ����� JS
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // ������������ secure cookies, ���� ���������� �������� (https)
                options.Cookie.SameSite = SameSiteMode.Strict; // ������� �������� SameSite
                options.Cookie.MaxAge = TimeSpan.FromDays(30); // ����� ����� cookie
                options.ExpireTimeSpan = TimeSpan.FromDays(30); // ����� ����� ������

                // ��������� ��� ����� � ������
                options.LoginPath = "/Account/Login";  // ���� � �������� �����
                options.LogoutPath = "/Account/Logout"; // ���� � �������� ������
                options.AccessDeniedPath = "/Account/AccessDenied"; // ���� ��� ������� ��� ������
            });


            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseMigrationsEndPoint();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}
            //app.Use(async (context, next) =>
            //{
            //    context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 104857600; // 100MB
            //    await next.Invoke();
            //});

     


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=CurrentProjects}/{action=Index}");
            app.MapRazorPages();

            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    var userManager = services.GetRequiredService<UserManager<ErpUser>>();
            //    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            //    var context = services.GetRequiredService<ErpContext>();

            //    // Ensure the database is up-to-date
            //    await context.Database.MigrateAsync();

            //    string adminRole = "Boss";
            //    if (!await roleManager.RoleExistsAsync(adminRole))
            //    {
            //        await roleManager.CreateAsync(new IdentityRole(adminRole));
            //    }

            //    const string bossEmail = "natalija.mamaeva@yandex.ru";
            //    const string bossPhone = "+79153144204";
            //    const string bossName = "������� ���������� �������";
            //    const string bossPassword = "SecurePassword123!"; // ���������� ������� ������

            //    var bossUser = await userManager.FindByEmailAsync(bossEmail);
            //    if (bossUser == null)
            //    {
            //        Employee employee = new Employee
            //        {
            //            EmployeeName = bossName,
            //            PhoneNumber = bossPhone,
            //            Email = bossEmail,
            //            Position = "Boss"
            //        };

            //        bossUser = new ErpUser
            //        {
            //            UserName = bossEmail,
            //            Email = bossEmail,
            //            PhoneNumber = bossPhone,
            //            Employee = employee
            //        };

            //        context.Employees.Add(employee);
            //        await context.SaveChangesAsync();  // Save employee before creating the user

            //        var result = await userManager.CreateAsync(bossUser, bossPassword);
            //        if (result.Succeeded)
            //        {
            //            await userManager.AddToRoleAsync(bossUser, adminRole);  // Make sure role matches
            //            Console.WriteLine("������ ������������ 'Boss' ������� ������.");
            //        }
            //        else
            //        {
            //            Console.WriteLine("������ �������� ������������:");
            //            foreach (var error in result.Errors)
            //            {
            //                Console.WriteLine($"��� ������: {error.Code}, ��������: {error.Description}");
            //            }
            //        }
            //    }
            //}

            app.Run();
        }
    }
}
