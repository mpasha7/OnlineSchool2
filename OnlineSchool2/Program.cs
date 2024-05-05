using Microsoft.EntityFrameworkCore;
using OnlineSchool2.Models;

namespace OnlineSchool2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<SchoolContext>(opts =>
            {
                opts.UseSqlServer(builder.Configuration.GetConnectionString("SchoolConnection"));
                opts.EnableSensitiveDataLogging(true);
            });
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
            var app = builder.Build();


            
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            SeedData.SeedDatabase(app);
            app.Run();
        }
    }
}
