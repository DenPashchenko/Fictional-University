using FictionalUniversityWebApp.Models;

namespace FictionalUniversityWebApp.DAL.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<Course> CoursesRepository { get; }
        IGenericRepository<Group> GroupsRepository { get; }
        IGenericRepository<Student> StudentsRepository { get; }

        Task SaveAsync();
    }
}