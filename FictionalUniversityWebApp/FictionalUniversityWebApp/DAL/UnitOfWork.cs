using FictionalUniversityWebApp.Models;
using FictionalUniversityWebApp.Data;
using FictionalUniversityWebApp.DAL.Interfaces;

namespace TestWebApplication.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private EducationDBContext context;
        private IGenericRepository<Course> coursesRepository;
        private IGenericRepository<Group> groupsRepository;
        private IGenericRepository<Student> studentsRepository;

        public IGenericRepository<Course> CoursesRepository
        {
            get
            {
                if (this.coursesRepository == null)
                {
                    this.coursesRepository = new GenericRepository<Course>(context);
                }
                return coursesRepository;
            }
        }

        public IGenericRepository<Group> GroupsRepository
        {
            get
            {
                if (this.groupsRepository == null)
                {
                    this.groupsRepository = new GenericRepository<Group>(context);
                }
                return groupsRepository;
            }
        }

        public IGenericRepository<Student> StudentsRepository
        {
            get
            {
                if (this.studentsRepository == null)
                {
                    this.studentsRepository = new GenericRepository<Student>(context);
                }
                return studentsRepository;
            }
        }

        public UnitOfWork(EducationDBContext context)
        {
            this.context = context;
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}