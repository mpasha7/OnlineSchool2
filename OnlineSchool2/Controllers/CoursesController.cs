using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineSchool2.Models;

namespace OnlineSchool2.Controllers
{
    [Authorize(Roles = "Coach,Student")]    
    public class CoursesController : Controller
    {
        private readonly SchoolContext db;
        private IWebHostEnvironment env;
        private UserManager<IdentityUser> userManager;

        public CoursesController(SchoolContext context, IWebHostEnvironment env, UserManager<IdentityUser> usrMgr)
        {
            this.db = context;
            this.env = env;
            this.userManager = usrMgr;
        }

        // GET: Courses        
        public async Task<IActionResult> Index()
        {
            ViewBag.IsCoach = User.IsInRole("Coach");
            string? userId = (await userManager.FindByNameAsync(User.Identity.Name))?.Id;
            List<Course> courses;
            if (ViewBag.IsCoach)
            {
                courses = await db.Courses.Where(c => c.CoachGuid == userId)
                    .OrderByDescending(c => c.CreatedDate).ToListAsync();
            }
            else
            {
                courses = await db.Courses.Include(c => c.Students).Where(c => c.Students.Where(s => s.StudentGuid == userId).Any())
                    .OrderByDescending(c => c.CreatedDate).ToListAsync();
            }
            return View(courses);
        }

        // GET: Courses/Create
        [Authorize(Roles = "Coach")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coach")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,PhotoPath,BeginQuestionnaire,EndQuestionnaire")] Course course)
        {
            if (ModelState.IsValid)
            {
                IFormFileCollection files = HttpContext.Request.Form.Files;
                foreach (var file in files)
                {
                    DateTime d = DateTime.Now;
                    string path = $"/photos/{d.ToString("yy_MM_dd")}_{d.ToString("hh_mm_ss")}_{course.Title + Path.GetExtension(file.FileName)}";
                    using (var fileStream = new FileStream(env.WebRootPath + path, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    course.PhotoPath = path;
                }
                course.CoachGuid = (await userManager.FindByNameAsync(User.Identity.Name))?.Id;
                await db.Courses.AddAsync(course);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Coach")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewData["OldPath"] = course.PhotoPath;
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coach")]
        public async Task<IActionResult> Edit(string path, int id, [Bind("Id,Title,Description,PhotoPath,BeginQuestionnaire,EndQuestionnaire")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {                
                IFormFileCollection files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    DeletePhoto(path);
                    foreach (var file in files)
                    {
                        DateTime d = DateTime.Now;
                        path = $"/photos/{d.ToString("yy_MM_dd")}_{d.ToString("hh_mm_ss")}_{course.Title + Path.GetExtension(file.FileName)}";
                        using (var fileStream = new FileStream(env.WebRootPath + path, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                    }
                }                
                course.PhotoPath = path;
                course.CoachGuid = (await userManager.FindByNameAsync(User.Identity.Name))?.Id;

                try
                {
                    db.Update(course);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        public async Task<IActionResult> GetQuestionnaire(int courseid, bool isbegin)
        {
            Course? course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseid);
            if (course is null)
            {
                return NotFound();
            }
            ViewBag.IsBegin = isbegin;
            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Coach")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await db.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coach")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id is null || db.Courses == null)
            {
                return NotFound();
            }

            var course = await db.Courses.FindAsync(id);
            if (course != null)
            {
                string? photoPath = course.PhotoPath;
                DeletePhoto(photoPath);

                db.Courses.Remove(course);
                await db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void DeletePhoto(string? photoPath)
        {
            if (photoPath is not null)
            {
                FileInfo file = new FileInfo(env.WebRootPath + photoPath);
                if (file.Exists)
                {
                    file.Delete();
                }
            }
        }

        private bool CourseExists(int id)
        {
            return db.Courses.Any(e => e.Id == id);
        }
    }
}
