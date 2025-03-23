//using ERP.Data;
using ERP.Models;
using ERP.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;


using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Configuration;


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
            builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);


            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ErpContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddHttpClient();

            builder.Services.AddIdentity<ErpUser, IdentityRole>()
                .AddEntityFrameworkStores<ErpContext>()
                .AddDefaultTokenProviders(); // ��������� ������ ��� ������ ������ � ������ ������� 

            //builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            //var configJson = File.ReadAllText("appsettings.json");

            builder.Services.AddHostedService<YandexTokenRefreshService>();

            builder.Services.Configure<YandexDiskSettings>(builder.Configuration.GetSection("YandexDisk"));
            builder.Services.AddScoped<EmailSender>();

            var yandexSettings = builder.Configuration.GetSection("YandexDisk").Get<YandexDiskSettings>();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();  // ��� ������ ����� � �������
            builder.Logging.AddFile("Logs/app-log-{Date}.txt"); // ��� ������ ����� � ����

            builder.Services.AddDataProtection()
                        .PersistKeysToFileSystem(new DirectoryInfo(@"Keys"));

          

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

           

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = null; // ��������� null ��� ���������� ����������� ����������� ������
            });


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

        



            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream",
            });

           
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=CurrentProjects}/{action=Index}");
            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<ErpUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var context = services.GetRequiredService<ErpContext>();

                // Ensure the database is up-to-date
                await context.Database.MigrateAsync();

                string adminRole = "Boss";
                if (!await roleManager.RoleExistsAsync(adminRole))
                {
                    await roleManager.CreateAsync(new IdentityRole(adminRole));
                }

                const string bossEmail = "natalija.mamaeva@yandex.ru";
                const string bossPhone = "+79153144204";
                const string bossName = "������� ���������� �������";
                const string bossPassword = "SecurePassword123!"; // ���������� ������� ������

                var bossUser = await userManager.FindByEmailAsync(bossEmail);
                if (bossUser == null)
                {
                    Employee employee = new Employee
                    {
                        EmployeeName = bossName,
                        PhoneNumber = bossPhone,
                        Email = bossEmail,
                        Position = "Boss"
                    };

                    bossUser = new ErpUser
                    {
                        UserName = bossEmail,
                        Email = bossEmail,
                        PhoneNumber = bossPhone,
                        Employee = employee
                    };

                    context.Employees.Add(employee);
                    await context.SaveChangesAsync();  // Save employee before creating the user

                    var result = await userManager.CreateAsync(bossUser, bossPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(bossUser, adminRole);  // Make sure role matches
                        Console.WriteLine("������ ������������ 'Boss' ������� ������.");
                    }
                    else
                    {
                        Console.WriteLine("������ �������� ������������:");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"��� ������: {error.Code}, ��������: {error.Description}");
                        }
                    }
                }
            }
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                string master = "Master";
                if (!await roleManager.RoleExistsAsync(master))
                {
                    await roleManager.CreateAsync(new IdentityRole(master));
                }
                string designer = "Designer";
                if (!await roleManager.RoleExistsAsync(designer))
                {
                    await roleManager.CreateAsync(new IdentityRole(designer));
                }
                string phoneManager = "PhoneManager";
                if (!await roleManager.RoleExistsAsync(phoneManager))
                {
                    await roleManager.CreateAsync(new IdentityRole(phoneManager));
                }
            }

            app.Run();

          

        }
    }
}
