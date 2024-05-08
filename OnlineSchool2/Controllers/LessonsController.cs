using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineSchool2.Models;

namespace OnlineSchool2.Controllers
{
    public class LessonsController : Controller
    {
        private readonly SchoolContext db;

        public LessonsController(SchoolContext context)
        {
            db = context;
        }

        // GET: Lessons
        public async Task<IActionResult> Index(int? courseid)
        {
            LessonsListViewModel model = new LessonsListViewModel
            {
                Lessons = await db.Lessons.Where(l => l.Course.Id == courseid).ToListAsync(),
                Course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseid)
            };
            return View(model);
        }

        // GET: Lessons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await db.Lessons.Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // GET: Lessons/Create
        public IActionResult Create(int? courseid)
        {
            return View(new LessonViewModel
            {
                Lesson = new Lesson(),
                Course = db.Courses.FirstOrDefault(c => c.Id == courseid)
            });
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? courseid, [Bind("Id,Title,Description,VideoLink")] Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                Course? course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseid);
                lesson.Course = course;
                lesson.Id = default;

                db.Add(lesson);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { courseid = courseid });
            }
            return View(lesson);
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await db.Lessons.Include(l => l.Course).FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? courseid, int id, [Bind("Id,Title,Description,VideoLink")] Lesson lesson)
        {
            if (id != lesson.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Course? course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseid);
                    lesson.Course = course;

                    db.Update(lesson);
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(lesson.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { courseid = courseid });
            }
            return View(lesson);
        }

        // GET: Lessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await db.Lessons.Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? courseid, int id)
        {
            var lesson = await db.Lessons.FindAsync(id);
            if (lesson != null)
            {
                db.Lessons.Remove(lesson);
            }

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { courseid = courseid });
        }

        private bool LessonExists(int id)
        {
            return db.Lessons.Any(e => e.Id == id);
        }
    }
}
