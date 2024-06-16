using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineSchool2.Controllers;
using OnlineSchool2.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace OnlineSchool2.Tests
{
    public class CoachTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient? _client;
        private ServiceProvider _provider;

        public CoachTests(WebApplicationFactory<Program> factory)
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
        public async void CoachScriptTest()
        {
            // Вход с неверным паролем. Возврат на форму
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["Name"] = "Irina",
                ["Password"] = "Irona123$",                                // Неверный пароль
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

            // Вход с верным паролем. Редирект на список своих курсов
            data["Password"] = "Irina123$";                                // Верный пароль
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();            // Редирект на ранее запрошенную страницу с курсами этого тренера
            regex = new Regex(@"(?<=/Lessons\?courseid=)\d+", RegexOptions.IgnoreCase);
            string[] numbers = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(3, numbers.Length);
            Assert.DoesNotContain("1", numbers);                          // Id чужого курса
            Assert.Contains("2", numbers);                                // Id своего курса
            Assert.Contains("3", numbers);                                // Id своего курса
            Assert.Contains("4", numbers);                                // Id своего курса
            Assert.DoesNotContain("5", numbers);                          // Id несуществующего курса
            // Проверка наличия управляющих кнопок
            regex = new Regex(@"(?<=href="").+?(?="")", RegexOptions.IgnoreCase);
            string[] links = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Contains("/Courses/Create", links);                    // Кнопка для создания курса
            Assert.Contains("/Courses/Edit/2", links);                    // Кнопка для редактирования курса
            Assert.Contains("/Courses/Delete/2", links);                  // Кнопка для удаления курса
            Assert.Contains("/Students/List", links);                     // Кнопка управления своими ученикамми

            // Создание курса. Редирект на обновленный список курсов
            var multiForm = new MultipartFormDataContent();
            var image = await GetTestImage("test_photo.png");
            multiForm.Add(new StreamContent(image), "PhotoPath", "test_photo.png");
            multiForm.Add(new StringContent("Course5"), "Title");
            multiForm.Add(new StringContent("Description5"), "Description");
            multiForm.Add(new StringContent("ShortDescription5"), "ShortDescription");
            multiForm.Add(new StringContent("PublicDescription5"), "PublicDescription");
            multiForm.Add(new StringContent("BeginQuestionnaire5"), "BeginQuestionnaire");
            multiForm.Add(new StringContent("EndQuestionnaire5"), "EndQuestionnaire");
            response = await _client.PostAsync("/Courses/Create", multiForm);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();            // Редирект на страницу с курсами тренера
            regex = new Regex(@"(?<=/Lessons\?courseid=)\d+", RegexOptions.IgnoreCase);
            numbers = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(4, numbers.Length);
            Assert.DoesNotContain("1", numbers);                          // Id чужого курса
            Assert.Contains("2", numbers);                                // Id своего курса
            Assert.Contains("3", numbers);                                // Id своего курса
            Assert.Contains("4", numbers);                                // Id своего курса
            Assert.Contains("5", numbers);                                // Id НОВОГО курса
            // Проверка загруженного файла
            regex = new Regex(@"(?<=src="")/photos/.+?Course5.+?(?="")", RegexOptions.IgnoreCase);
            string[] files = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(files);                                         // Файл фото
            var fileResponse = await _client.GetAsync(files[0]);
            Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);     // Запрос к загруженному файлу фото
            // Проверка созданного курса
            IActionResult result;
            using (var scope = _provider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var ctx = scopedServices.GetRequiredService<SchoolContext>();
                result = new HomeController(ctx).Index();
            }            
            var viewResult = Assert.IsType<ViewResult>(result);
            var courses = Assert.IsType<List<Course>>(viewResult.ViewData.Model);
            Course course = courses.SingleOrDefault(c => c.Id == 5);
            Assert.NotNull(course);
            Assert.Equal("Course5", course.Title);
            Assert.Equal("Description5", course.Description);
            Assert.Equal("ShortDescription5", course.ShortDescription);
            Assert.Equal("PublicDescription5", course.PublicDescription);
            Assert.Equal("BeginQuestionnaire5", course.BeginQuestionnaire);
            Assert.Equal("EndQuestionnaire5", course.EndQuestionnaire);

            // Редактирование курса. Редирект на обновленный список курсов
            multiForm = new MultipartFormDataContent();
            image = await GetTestImage("changed_photo.png");
            multiForm.Add(new StreamContent(image), "PhotoPath", "changed_photo.png");
            multiForm.Add(new StringContent("Course5NEW"), "Title");
            multiForm.Add(new StringContent("Description5NEW"), "Description");
            multiForm.Add(new StringContent("ShortDescription5NEW"), "ShortDescription");
            multiForm.Add(new StringContent("PublicDescription5NEW"), "PublicDescription");
            multiForm.Add(new StringContent("BeginQuestionnaire5NEW"), "BeginQuestionnaire");
            multiForm.Add(new StringContent("EndQuestionnaire5NEW"), "EndQuestionnaire");
            response = await _client.PostAsync($"/Courses/Edit/5?path={files[0]}", multiForm);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();               // Редирект на страницу с курсами тренера
            regex = new Regex(@"(?<=/Lessons\?courseid=)\d+", RegexOptions.IgnoreCase);
            numbers = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(4, numbers.Length);
            Assert.Contains("5", numbers);                                   // Id отредактированного курса
            // Проверка нового загруженного файла
            regex = new Regex(@"(?<=src="")/photos/.+?Course5.+?(?="")", RegexOptions.IgnoreCase);
            string[] newFiles = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(newFiles);                                         // Новый файл фото
            var newFileResponse = await _client.GetAsync(newFiles[0]);
            Assert.Equal(HttpStatusCode.OK, newFileResponse.StatusCode);     // Запрос к отредактированному файлу фото
            fileResponse = await _client.GetAsync(files[0]);
            Assert.Equal(HttpStatusCode.NotFound, fileResponse.StatusCode);  // Запрос к старому файлу фото
            // Проверка отредактированного курса
            using (var scope = _provider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var ctx = scopedServices.GetRequiredService<SchoolContext>();
                result = new HomeController(ctx).Index();
            }
            viewResult = Assert.IsType<ViewResult>(result);
            courses = Assert.IsType<List<Course>>(viewResult.ViewData.Model);
            course = courses.SingleOrDefault(c => c.Id == 5);
            Assert.NotNull(course);
            Assert.Equal("Course5NEW", course.Title);
            Assert.Equal("Description5NEW", course.Description);
            Assert.Equal("ShortDescription5NEW", course.ShortDescription);
            Assert.Equal("PublicDescription5NEW", course.PublicDescription);
            Assert.Equal("BeginQuestionnaire5NEW", course.BeginQuestionnaire);
            Assert.Equal("EndQuestionnaire5NEW", course.EndQuestionnaire);            

            // Переход к списку уроков курса
            response = await _client.GetAsync("/Lessons?courseid=5");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();              // Страница с уроками курса (courseId = 5)
            regex = new Regex(@"(?<=/Lessons/Details/)\d+", RegexOptions.IgnoreCase);
            numbers = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Empty(numbers);                                          // В курсе НЕТ уроков

            // Добавление урока в курс
            data = new Dictionary<string, string>
            {
                ["Number"] = "1",
                ["Title"] = "Title 1",
                ["Description"] = "Description 1",
                ["VideoLink"] = "VideoLink 1"
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync("/Lessons/Create?courseid=5&maxnumber=1", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();             // Редирект на страницу с уроками курса
            regex = new Regex(@"(?<=/Lessons/Details/)\d+", RegexOptions.IgnoreCase);
            numbers = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(numbers);
            Assert.Equal("13", numbers[0]);
            regex = new Regex(@"(?<=<h2.*?>)\d+\. Title \d+(?=</h2>)", RegexOptions.IgnoreCase);
            string[] titles = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(titles);                                          // В курсе 1 урок
            Assert.Equal("1. Title 1", titles[0]);

            // Добавление второго урока в курс
            data = new Dictionary<string, string>
            {
                ["Number"] = "2",
                ["Title"] = "Title 2",
                ["Description"] = "Description 2",
                ["VideoLink"] = "VideoLink 2"
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync("/Lessons/Create?courseid=5&maxnumber=2", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();             // Редирект на страницу с уроками курса
            regex = new Regex(@"(?<=/Lessons/Details/)\d+", RegexOptions.IgnoreCase);
            numbers = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(2, numbers.Length);
            Assert.Contains("13", numbers);
            Assert.Contains("14", numbers);
            regex = new Regex(@"(?<=<h2.*?>)\d+\. Title \d+(?=</h2>)", RegexOptions.IgnoreCase);
            titles = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(2, titles.Length);                               // В курсе 2 урока
            Assert.Contains("1. Title 1", titles);
            Assert.Contains("2. Title 2", titles);

            // Изменение порядка уроков
            data = new Dictionary<string, string>
            {
                ["Id"] = "14",
                ["Number"] = "1",
                ["Title"] = "Title 2",
                ["Description"] = "Description 2",
                ["VideoLink"] = "VideoLink 2"
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync("/Lessons/Edit/14?courseid=5&oldnumber=2", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();             // Редирект на страницу с уроками курса
            regex = new Regex(@"(?<=<h2.*?>)\d+\. Title \d+(?=</h2>)", RegexOptions.IgnoreCase);
            titles = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(2, titles.Length);                               // В курсе 2 урока
            Assert.Contains("1. Title 2", titles);
            Assert.Contains("2. Title 1", titles);

            // Удаление урока
            data = new Dictionary<string, string>
            {
                ["Id"] = "14"
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync("/Lessons/Delete/14?courseid=5&oldnumber=1", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();             // Редирект на страницу с уроками курса
            regex = new Regex(@"(?<=<h2.*?>)\d+\. Title \d+(?=</h2>)", RegexOptions.IgnoreCase);
            titles = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Single(titles);                                       // В курсе 1 урок
            Assert.Contains("1. Title 1", titles);

            // Удаление курса
            data = new Dictionary<string, string>
            {
                ["Id"] = "5"
            };
            form = new FormUrlEncodedContent(data);
            response = await _client.PostAsync($"/Courses/Delete/5", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync();               // Редирект на страницу с курсами тренера
            regex = new Regex(@"(?<=/Lessons\?courseid=)\d+", RegexOptions.IgnoreCase);
            numbers = regex.Matches(html).Cast<Match>().Select(m => m.Value).ToArray();
            Assert.Equal(3, numbers.Length);
            Assert.Contains("2", numbers);                                // Id своего курса
            Assert.Contains("3", numbers);                                // Id своего курса
            Assert.Contains("4", numbers);                                // Id своего курса
            Assert.DoesNotContain("5", numbers);                          // Id удаленного курса
            // Проверка отсутствия файла с фото удаленного курса            
            newFileResponse = await _client.GetAsync(files[0]);
            Assert.Equal(HttpStatusCode.NotFound, newFileResponse.StatusCode);  // Запрос к файлу фото удаленного курса
            // Проверка отредактированного курса
            using (var scope = _provider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var ctx = scopedServices.GetRequiredService<SchoolContext>();
                result = new HomeController(ctx).Index();
            }
            viewResult = Assert.IsType<ViewResult>(result);
            courses = Assert.IsType<List<Course>>(viewResult.ViewData.Model);
            course = courses.SingleOrDefault(c => c.Id == 5);
            Assert.Null(course);                                          // Оставшиеся уроки каскадно удалены
        }

        private async Task<Stream> GetTestImage(string filename)
        {
            var memoryStream = new MemoryStream();
            var fileStream = File.OpenRead(filename);
            await fileStream.CopyToAsync(memoryStream);
            fileStream.Close();
            return memoryStream;
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
