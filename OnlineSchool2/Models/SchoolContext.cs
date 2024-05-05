using Microsoft.EntityFrameworkCore;

namespace OnlineSchool2.Models
{
    public class SchoolContext : DbContext
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }

        public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
        {
            
        }
    }
}
