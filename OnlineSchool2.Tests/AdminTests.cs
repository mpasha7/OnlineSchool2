using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineSchool2.Controllers;
using OnlineSchool2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnlineSchool2.Tests
{
    public class AdminTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient? _client;
        private ServiceProvider _provider;

        public AdminTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                var servicesToRemove = new[]
                {
                    typeof(DbContextOptions<SchoolContext>),
                    typeof(DbContextOptions<AppIdentityDbContext>)
                };
                foreach (var serviceType in servicesToRemove)
                {
                    var service = services.SingleOrDefault(d => d.ServiceType == serviceType);
                    services.Remove(service);
                }
                string schoolConnection = "Server=(localdb)\\MSSQLLocalDB;Database=CoursesTestDb;MultipleActiveResultSets=true";
                services.AddDbContext<SchoolContext>(options => options.UseSqlServer(schoolConnection));
                string identityConnection = "Server=(localdb)\\MSSQLLocalDB;Database=IdentityTestDb;MultipleActiveResultSets=true";
                services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(identityConnection));

                _provider = services.BuildServiceProvider();
                using (var scope = _provider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var identityDb = scopedServices.GetRequiredService<AppIdentityDbContext>();
                    identityDb.Database.EnsureCreated();
                }
            }));

            _client = _factory.CreateClient();
        }

        [Fact]
        public async void AdminScriptTest()
        {
            // Вход. Редирект на страницу со списком пользователей
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["Name"] = "Poul",
                ["Password"] = "Poul123$",
                ["ReturnUrl"] = "/Users/List"
            };
            HttpContent form = new FormUrlEncodedContent(data);
            var response = await _client.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();            // Страница со списком пользователей
            Regex regex = new Regex(@"(?<=<td>).+?(?=</td>)", RegexOptions.IgnoreCase);
            string[] ceils = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Poul", ceils);                                   // Список всех пользователей
            Assert.Contains("Irina", ceils);
            Assert.Contains("Olga", ceils);
            Assert.Contains("Alex", ceils);
            Assert.Contains("Tom", ceils);
            regex = new Regex(@"(?<=<a\s.*?>.*?<h5>).+?(?=</h5></a>)", RegexOptions.IgnoreCase);
            string[] logButtons = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Выход", logButtons);
            Assert.DoesNotContain("Вход", logButtons);

            //// Создание пользователя
            string roleId;
            using (var scope = _provider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var manager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
                roleId = (await manager.FindByNameAsync("Student")).Id;
            }
            data = new Dictionary<string, string>
            {
                ["UserName"] = "User",
                ["Email"] = "user@gmail.com",
                ["Password"] = "User123$",
                ["ConfirmPassword"] = "User123$",
                ["RoleId"] = roleId
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync("/Users/Create", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();            // Редирект на страницу со списком пользователей
            regex = new Regex(@"(?<=<td>).+?(?=</td>)", RegexOptions.IgnoreCase);
            ceils = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("User", ceils);                               // Новый пользователь

            // Редактирование пользователя
            data = new Dictionary<string, string>
            {
                ["UserName"] = "Userr",
                ["Email"] = "userr@gmail.com",
                ["Password"] = "Userr123$",
                ["ConfirmPassword"] = "Userr123$"
            };
            form = new FormUrlEncodedContent(data);
            string userId;
            using (var scope = _provider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var manager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();
                userId = (await manager.FindByNameAsync("User")).Id;
            }
            response = await _client.PostAsync($"/Users/Editor/{userId}", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();                 // Редирект на страницу со списком пользователей
            regex = new Regex(@"(?<=<td>).+?(?=</td>)", RegexOptions.IgnoreCase);
            ceils = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.DoesNotContain("User", ceils);
            Assert.Contains("Userr", ceils);                                   // Пользователь с новым именем

            // Вход нового пользователя с ролью Student
            var userClient = _factory.CreateClient();
            data = new Dictionary<string, string>
            {
                ["Name"] = "Userr",
                ["Password"] = "Userr123$",
                ["ReturnUrl"] = "/Courses"
            };
            form = new FormUrlEncodedContent(data);
            response = await userClient.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();
            regex = new Regex(@"(?<=<a\s.*?>.*?<h5>).+?(?=</h5></a>)", RegexOptions.IgnoreCase);
            logButtons = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Выход", logButtons);
            Assert.DoesNotContain("Вход", logButtons);
            // Проверка НЕДОСТУПНОСТИ управляющих кнопок
            regex = new Regex(@"(?<=href="").+?(?="")", RegexOptions.IgnoreCase);
            string[] links = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.DoesNotContain("/Courses/Create", links);              // Кнопка для создания курса
            Assert.DoesNotContain("/Students/List", links);               // Кнопка управления своими ученикамми
            // Выход нового пользователя с ролью Student
            response = await userClient.GetAsync("/Account/Logout");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();
            regex = new Regex(@"(?<=<a\s.*?>.*?<h5>).+?(?=</h5></a>)", RegexOptions.IgnoreCase);
            logButtons = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Вход", logButtons);
            Assert.DoesNotContain("Выход", logButtons);

            // Добавление роли Тренера новому пользователю
            data = new Dictionary<string, string>
            {
                ["rolename"] = "Coach"
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync($"/Roles/Editor/{userId}?userid={userId}", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();
            regex = new Regex(@"(?<=<td>).+?(?=</td>)", RegexOptions.IgnoreCase);
            ceils = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Irina", ceils);                                   // Список всех пользователей с ролью Coach
            Assert.Contains("Olga", ceils);
            Assert.Contains("Userr", ceils);

            // Вход нового пользователя с ролью Coach
            data = new Dictionary<string, string>
            {
                ["Name"] = "Userr",
                ["Password"] = "Userr123$",
                ["ReturnUrl"] = "/Courses"
            };
            form = new FormUrlEncodedContent(data);
            response = await userClient.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();
            // Проверка ДОСТУПНОСТИ управляющих кнопок
            regex = new Regex(@"(?<=href="").+?(?="")", RegexOptions.IgnoreCase);
            links = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("/Courses/Create", links);              // Кнопка для создания курса
            Assert.Contains("/Students/List", links);               // Кнопка управления своими ученикамми
            // Выход нового пользователя с ролью Coach
            response = await userClient.GetAsync("/Account/Logout");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Удаление нового пользователя
            data = new Dictionary<string, string>
            {
                ["Id"] = userId
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync($"/Users/List?handler=Delete", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();
            regex = new Regex(@"(?<=<td>).+?(?=</td>)", RegexOptions.IgnoreCase);
            ceils = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Irina", ceils);                                   // Список всех пользователей с ролью Coach
            Assert.Contains("Olga", ceils);
            Assert.DoesNotContain("Userr", ceils);

            // Попытка входа удаленного пользователя
            data = new Dictionary<string, string>
            {
                ["Name"] = "Userr",
                ["Password"] = "Userr123$",
                ["ReturnUrl"] = "/Courses"
            };
            form = new FormUrlEncodedContent(data);
            response = await userClient.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();
            regex = new Regex(@"(?<=<input.+?name="").+?(?="".+?/>)", RegexOptions.IgnoreCase);
            string[] fields = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(4, fields.Length);
            Assert.Contains("ReturnUrl", fields);
            Assert.Contains("Name", fields);
            Assert.Contains("Password", fields);
            regex = new Regex(@"(?<=<a\s.*?>.*?<h5>).+?(?=</h5></a>)", RegexOptions.IgnoreCase);
            logButtons = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Вход", logButtons);
            Assert.DoesNotContain("Выход", logButtons);
        }

        public void Dispose()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var ctx = sp.GetRequiredService<SchoolContext>();
                var ctx2 = sp.GetRequiredService<AppIdentityDbContext>();
                ctx.Database.EnsureDeleted();
                ctx2.Database.EnsureDeleted();
            }
        }
    }
}
