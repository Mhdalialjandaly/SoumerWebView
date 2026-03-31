using DataAccess;
using DataAccess.Entities;
using DataAccess.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Services;
using SoumerMVCView.Services;

namespace SoumerMVCView
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
      
            builder.Services.AddIInjectableDependencies(builder.Configuration);
            // Other Services
            builder.Services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
            // ثانياً: تسجيل Identity
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                // إعدادات كلمة المرور
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // إعدادات المستخدم
                options.User.RequireUniqueEmail = true;

                // إعدادات تسجيل الدخول
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ثالثاً: إعدادات الـ Controllers مع الـ Authorization
            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            // رابعاً: إضافة Razor Pages إذا لزم الأمر
            builder.Services.AddRazorPages();

            var app = builder.Build();

            using (var context = new ApplicationDbContext())
            {
                context.Database.MigrateAsync();
            }

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); 
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.MapControllerRoute(
                name: "account",
                pattern: "Account/{action=Login}/{id?}",
                defaults: new { controller = "Account" })
                .AllowAnonymous();

            app.Run();
        }
    }
}
