using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineSchool2.Models;

namespace OnlineSchool2.Controllers
{
    [Authorize(Roles = "Coach,Student")]
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
                Lessons = await db.Lessons.Where(l => l.Course.Id == courseid).OrderBy(l => l.Number).ToListAsync(),
                Course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseid)
            };
            ViewBag.IsCoach = User.IsInRole("Coach");
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
            ViewBag.IsCoach = User.IsInRole("Coach");
            return View(lesson);
        }

        // GET: Lessons/Create
        [Authorize(Roles = "Coach")]
        public IActionResult Create(int? courseid)
        {
            int? maxNumber = (db.Lessons.Where(l => l.Course.Id == courseid).Max(l => l.Number) ?? 0) + 1;
            return View(new LessonViewModel
            {
                Lesson = new Lesson { Number = maxNumber },
                Course = db.Courses.FirstOrDefault(c => c.Id == courseid),
                TargetNumber = maxNumber
            });
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coach")]
        public async Task<IActionResult> Create(int courseid, int maxnumber, [Bind("Id,Number,Title,Description,VideoLink")] Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                if (lesson.Number is null)
                {
                    lesson.Number = maxnumber;
                }
                if (lesson.Number < maxnumber)
                {
                    await LessonNumbersShiftRight(lesson.Number, courseid);
                }

                Course? course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseid);
                lesson.Course = course;
                lesson.Id = default;

                db.Add(lesson);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { courseid = courseid });
            }
            return View(lesson);
        }

        private async Task LessonNumbersShiftRight(int? lesNumber, int courseid)
        {
            var shiftLessons = db.Lessons.Where(l => l.Course.Id == courseid && l.Number >= lesNumber);
            foreach (var les in shiftLessons)
            {
                les.Number++;
                db.Update(les);
            }
            await db.SaveChangesAsync();
        }

        [Authorize(Roles = "Coach")]
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
            return View(new LessonViewModel
            {
                Lesson = lesson,
                TargetNumber = lesson.Number
            });
        }

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coach")]
        public async Task<IActionResult> Edit(int? courseid, int oldnumber, int id, [Bind("Id,Number,Title,Description,VideoLink")] Lesson lesson)
        {
            if (id != lesson.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (lesson.Number is null)
                    {
                        lesson.Number = oldnumber;
                    }
                    int? lesNumber = lesson.Number;                    

                    Course? course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseid);
                    lesson.Course = course;
                    db.Update(lesson);
                    await db.SaveChangesAsync();

                    if (lesson.Number != oldnumber)
                    {
                        await LessonNumbersAdjust(lesNumber, oldnumber, courseid, lesson.Id);
                    }
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

        private async Task LessonNumbersAdjust(int? lesNumber, int? oldnumber, int? courseid, int id)
        {
            int? maxNumber = db.Lessons.Where(l => l.Course.Id == courseid).Max(l => l.Number);/////////
            if (lesNumber < oldnumber)
            {
                var shiftLessons = db.Lessons.Where(l => l.Course.Id == courseid && l.Id != id && l.Number >= lesNumber && l.Number < oldnumber);
                foreach (var les in shiftLessons)
                {
                    les.Number++;
                    db.Update(les);
                }
            }
            else if (lesNumber > oldnumber)
            {
                var shiftLessons = db.Lessons.Where(l => l.Course.Id == courseid && l.Id != id && l.Number <= lesNumber && l.Number > oldnumber);
                foreach (var les in shiftLessons)
                {
                    les.Number--;
                    db.Update(les);
                }
            }
            await db.SaveChangesAsync();
        }

        // GET: Lessons/Delete/5
        [Authorize(Roles = "Coach")]
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
        [Authorize(Roles = "Coach")]
        public async Task<IActionResult> DeleteConfirmed(int? courseid, int id)
        {
            var lesson = await db.Lessons.FindAsync(id);
            if (lesson != null)
            {
                await LessonNumbersShiftLeft(lesson.Number, courseid);
                db.Lessons.Remove(lesson);
            }
            await db.SaveChangesAsync();

            
            return RedirectToAction(nameof(Index), new { courseid = courseid });
        }

        private async Task LessonNumbersShiftLeft(int? oldnumber, int? courseid)
        {
            var shiftLessons = db.Lessons.Where(l => l.Course.Id == courseid && l.Number > oldnumber);
            foreach (var les in shiftLessons)
            {
                les.Number--;
                db.Update(les);
            }
            await db.SaveChangesAsync();
        }

        private bool LessonExists(int id)
        {
            return db.Lessons.Any(e => e.Id == id);
        }
    }
}
