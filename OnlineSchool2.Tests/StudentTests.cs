using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineSchool2.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace OnlineSchool2.Tests
{
    public class StudentTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient? _client;
        private ServiceProvider _provider;

        public StudentTests(WebApplicationFactory<Program> factory)
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
        public async void StudentScriptTest()
        {
            // Вход с неверным паролем. Возврат на форму
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["Name"] = "Tom",
                ["Password"] = "Tomm123$",                                // Неверный пароль
                ["ReturnUrl"] = "/Courses"
            };
            HttpContent form = new FormUrlEncodedContent(data);
            var response = await _client.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();         // Возврат на страницу с формой ввода
            Regex regex = new Regex(@"(?<=<input.+?name="").+?(?="".+?/>)", RegexOptions.IgnoreCase);
            string[] fields = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(4, fields.Length);
            Assert.Contains("ReturnUrl", fields);
            Assert.Contains("Name", fields);
            Assert.Contains("Password", fields);
            regex = new Regex(@"(?<=<a\s.*?>.*?<h5>).+?(?=</h5></a>)", RegexOptions.IgnoreCase);
            string[] logButtons = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Вход", logButtons);
            Assert.DoesNotContain("Выход", logButtons);

            // Вход с верным паролем. Редирект на список своих курсов
            data["Password"] = "Tom123$";                                 // Верный пароль
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();            // Редирект на ранее запрошенную страницу с курсами этого тренера
            regex = new Regex(@"(?<=/Lessons\?courseid=)\d+", RegexOptions.IgnoreCase);
            string[] numbers = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(2, numbers.Length);
            Assert.Contains("1", numbers);                                // Id приобретенного курса
            Assert.Contains("2", numbers);                                // Id приобретенного курса
            Assert.DoesNotContain("3", numbers);                          // Id НЕ приобретенного курса
            Assert.DoesNotContain("4", numbers);                          // Id НЕ приобретенного курса
            regex = new Regex(@"(?<=<a\s.*?>.*?<h5>).+?(?=</h5></a>)", RegexOptions.IgnoreCase);
            logButtons = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Выход", logButtons);
            Assert.DoesNotContain("Вход", logButtons);
            // Проверка недоступности управляющих кнопок
            regex = new Regex(@"(?<=href="").+?(?="")", RegexOptions.IgnoreCase);
            string[] links = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.DoesNotContain("/Courses/Create", links);              // Кнопка для создания курса
            Assert.DoesNotContain("/Students/List", links);               // Кнопка управления своими ученикамми


            // Переход к списку уроков курса
            response = await _client.GetAsync("/Lessons?courseid=1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();              // Страница с уроками курса (courseId = 1)
            regex = new Regex(@"(?<=/Lessons/Details/)\d+", RegexOptions.IgnoreCase);
            numbers = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(3, numbers.Length);                                          // В курсе 3 урока
            Assert.Contains("1", numbers);
            Assert.Contains("2", numbers);
            Assert.Contains("3", numbers);

            // Переход на страницу урока (Id = 1)
            response = await _client.GetAsync("/Lessons/Details/1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();              // Страница урока (Id = 1)
            regex = new Regex(@"(?<=<h2.*?>).+?(?=</h2>)", RegexOptions.IgnoreCase);
            string[] titles = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(titles);
            Assert.Contains("Lesson 1.1", titles);                          // Название урока (Id = 1)
            regex = new Regex(@"(?<=<iframe.*?src="").+?(?="".*?></iframe>)", RegexOptions.IgnoreCase);
            string[] videolinks = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(videolinks);
            Assert.Contains(@"https://vk.com/video_ext.php?oid=54023064&amp;id=456239088&amp;hd=2&amp;hash=ec3da24f3c3555ea", videolinks);// Ссылка на видео урока (Id = 1)

            // Переход на страницу профиля
            response = await _client.GetAsync("/Account/Profile");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();              // Страница профиля
            regex = new Regex(@"(?<=<h2.*?>Имя:\s).+?(?=</h2>)", RegexOptions.IgnoreCase);
            string[] names = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(names);
            Assert.Contains("Tom", names);                                  // Имя пользователя
            regex = new Regex(@"(?<=<h2.*?>Email:\s).+?(?=</h2>)", RegexOptions.IgnoreCase);
            string[] emails = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(emails);
            Assert.Contains("tom@gmail.com", emails);                                 // Email пользователя

            // Редактирование профиля
            data = new Dictionary<string, string>
            {
                ["UserName"] = "TomNEW",                 // Новое имя
                ["Email"] = "tomnew@gmail.com",          // Новый email
                ["OldPassword"] = "Tom123$",
                ["NewPassword"] = "Tom123$NEW",          // Новый пароль
                ["ConfirmPassword"] = "Tom123$NEW",
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync("/Account/EditProfile", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();                 // Редирект на страницу профиля
            regex = new Regex(@"(?<=<h2.*?>Имя:\s).+?(?=</h2>)", RegexOptions.IgnoreCase);
            names = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(names);
            Assert.Contains("TomNEW", names);                                  // Новое имя пользователя
            regex = new Regex(@"(?<=<h2.*?>Email:\s).+?(?=</h2>)", RegexOptions.IgnoreCase);
            emails = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(emails);
            Assert.Contains("tomnew@gmail.com", emails);                       // Новый Email пользователя

            // Выход
            response = await _client.GetAsync("/Account/Logout");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();                 // Редирект на страницу профиля
            regex = new Regex(@"(?<=<a\s.*?>.*?<h5>).+?(?=</h5></a>)", RegexOptions.IgnoreCase);
            logButtons = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Вход", logButtons);
            Assert.DoesNotContain("Выход", logButtons);

            // Вход с новым паролем
            data = new Dictionary<string, string>
            {
                ["Name"] = "TomNEW",
                ["Password"] = "Tom123$NEW",
                ["ReturnUrl"] = "/Courses"
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();            // Редирект на ранее запрошенную страницу с курсами этого тренера
            regex = new Regex(@"(?<=<a\s.*?>.*?<h5>).+?(?=</h5></a>)", RegexOptions.IgnoreCase);
            logButtons = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("Выход", logButtons);
            Assert.DoesNotContain("Вход", logButtons);
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
