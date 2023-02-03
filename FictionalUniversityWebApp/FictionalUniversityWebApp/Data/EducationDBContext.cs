using Microsoft.EntityFrameworkCore;
using FictionalUniversityWebApp.Models;

namespace FictionalUniversityWebApp.Data
{
    public partial class EducationDBContext : DbContext
    {
        public EducationDBContext() : base()
        {
        }

        public EducationDBContext(DbContextOptions<EducationDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Group> Groups { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=EducationDb");
            }
        }
    }
}
