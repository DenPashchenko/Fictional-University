using FictionalUniversityWebApp.Controllers;
using FictionalUniversityWebApp.Models;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using FictionalUniversityWebApp.DAL.Interfaces;

namespace FictionalUniversityWebAppTests
{
    public class StudentControllerTest
    {
        public static List<Student> studentList = new()
        {
            new Student
            {
                StudentId = 1,
                GroupId = 1,
                FirstName = "Ivan",
                LastName = "Ivanenko",
                Group = new Group()
                {
                    GroupId = 1,
                    CourseId = 1,
                    Name = "SR-01"
                }
            },
           new Student
            {
                StudentId = 2,
                GroupId = 1,
                FirstName = "Petro",
                LastName = "Ivanenko",
                Group = new Group()
                {
                    GroupId = 1,
                    CourseId = 1,
                    Name = "SR-01"
                }
            }
        };

        [Fact]
        public async Task Index_ViewResult_WithListOfStudentsOrderedByName()
        {
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
                It.IsAny<Expression<Func<Student, bool>>>(),
                It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
                It.IsAny<string>()))
                    .ReturnsAsync(studentList);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Student>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task StudentSelection_IdIsNull_NotFoundResult()
        {
            var controller = new StudentController(null);

            var result = await controller.StudentSelection(null);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task StudentSelection_StudentRepositoryIsNull_NotFoundResult()
        {
            int studentId = 1;
            IEnumerable<Student> nullStudent = null;
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository).Returns((IGenericRepository<Student>)nullStudent);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.StudentSelection(studentId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task StudentSelection_CourseId_ViewResultWithGroupsInCourse()
        {
            int studentId = 1;
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
                It.IsAny<Expression<Func<Student, bool>>>(),
                It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
                It.IsAny<string>()))
                    .ReturnsAsync(studentList);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.StudentSelection(studentId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Student>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_IdIsNull_NotFoundResult()
        {
            var controller = new StudentController(null);

            var result = await controller.Details(null);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_StudentIsNull_NotFoundResult()
        {
            int studentId = 1;
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
                It.IsAny<Expression<Func<Student, bool>>>(),
                It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
                It.IsAny<string>()))
                    .ReturnsAsync((IEnumerable<Student>)null);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.Details(studentId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_StudentId_ViewResultWithStudentDetails()
        {
            int studentId = 1;
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
                It.IsAny<Expression<Func<Student, bool>>>(),
                It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
                It.IsAny<string>()))
                    .ReturnsAsync(studentList);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.Details(studentId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Student>(viewResult.ViewData.Model);
            Assert.Equal(1, model.GroupId);
            Assert.Equal(1, model.StudentId);
            Assert.Equal("Ivan", model.FirstName);
            Assert.Equal("Ivanenko", model.LastName);
            Assert.IsType<Group>(model.Group);
        }

        [Fact]
        public async Task CreatePost_InvalidModelState_ReturnsTheSameView()
        {
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
                 It.IsAny<Expression<Func<Student, bool>>>(),
                 It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
                 It.IsAny<string>()))
                     .ReturnsAsync(studentList);
            mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
                It.IsAny<Expression<Func<Group, bool>>>(),
                It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
                It.IsAny<string>()))
                    .ReturnsAsync(GroupControllerTest.groupList);
            var controller = new StudentController(mockRepo.Object);
            controller.ModelState.AddModelError("SomeError", "Required");
            var newStudent = new Student();

            var result = await controller.Create(newStudent);
            var modelState = controller.ModelState;

            Assert.True(modelState.Keys.Contains("SomeError"));
            Assert.True(modelState["SomeError"].Errors.Count == 1);
            var invalidModelStateResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Student>(invalidModelStateResult.ViewData.Model);
        }

        [Fact]
        public async Task CreatePost_ValidModelState_AddedNewStudentAndRedirectedToIndex()
        {
            int studentId = 2;
            var newStudent = studentList.Find(g => g.GroupId == studentId);
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.Insert(newStudent));
            mockRepo.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask).Verifiable();
            var controller = new StudentController(mockRepo.Object);


            var result = await controller.Create(newStudent);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirectToActionResult.ControllerName);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mockRepo.Verify();
        }

        [Fact]
        public async Task EditGet_IdIsNull_NotFoundResult()
        {
            var controller = new StudentController(null);

            var result = await controller.Edit(null);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditGet_StudentIsNull_NotFoundResult()
        {
            int studentId = 1;
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
               It.IsAny<Expression<Func<Student, bool>>>(),
               It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
               It.IsAny<string>())).ReturnsAsync((IEnumerable<Student>)null);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.Edit(studentId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditGet_StudentId_ViewResultWithStudentDetails()
        {
            int studentId = 1;
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
               It.IsAny<Expression<Func<Student, bool>>>(),
               It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
               It.IsAny<string>()))
                .ReturnsAsync(studentList);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.Edit(studentId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Student>(viewResult.ViewData.Model);
            Assert.Equal(1, model.GroupId);
            Assert.Equal(1, model.StudentId);
            Assert.Equal("Ivan", model.FirstName);
            Assert.Equal("Ivanenko", model.LastName);
            Assert.IsType<Group>(model.Group);
        }

        [Fact]
        public async Task EditPost_InvalidStudentId_NotFoundResult()
        {
            int studentId = 1;
            int nextStudentId = 2;
            var student = studentList.Find(s => s.StudentId == studentId);
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
               It.IsAny<Expression<Func<Student, bool>>>(),
               It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
               It.IsAny<string>()))
                .ReturnsAsync(studentList);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.Edit(nextStudentId, student);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditPost_InvalidModelState_ReturnsTheSameView()
        {
            int studentId = 1;
            var student = studentList.Find(s => s.StudentId == studentId);
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
               It.IsAny<Expression<Func<Student, bool>>>(),
               It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
               It.IsAny<string>()))
                .ReturnsAsync(studentList);
            var controller = new StudentController(mockRepo.Object);
            controller.ModelState.AddModelError("SomeError", "Required");

            var result = await controller.Edit(studentId, student);
            var modelState = controller.ModelState;

            Assert.True(modelState.Keys.Contains("SomeError"));
            Assert.True(modelState["SomeError"].Errors.Count == 1);
            var invalidModelStateResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Student>(invalidModelStateResult.ViewData.Model);
        }

        [Fact]
        public async Task EditPost_ValidModelState_UpdatedStudentAndRedirectedToIndex()
        {
            int studentId = 2;
            var student = studentList.Find(s => s.StudentId == studentId);
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.Update(student));
            mockRepo.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask).Verifiable();
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.Edit(studentId, student);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirectToActionResult.ControllerName);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mockRepo.Verify();
        }

        [Fact]
        public async Task EditPost_ValidModelStateButDbUpdateConcurrencyExceptionAndStudentExists_ThrownException()
        {
            int studentId = 2;
            var student = studentList.Find(s => s.StudentId == studentId);
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
                It.IsAny<Expression<Func<Student, bool>>>(),
                It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
                It.IsAny<string>()))
                    .ReturnsAsync(studentList);
            mockRepo.Setup(repo => repo.StudentsRepository.Update(student))
                .Throws(new DbUpdateConcurrencyException(String.Empty, new List<IUpdateEntry> { Mock.Of<IUpdateEntry>() }));
            var controller = new StudentController(mockRepo.Object);

            Func<Task> act = () => controller.Edit(studentId, student);

            var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(act);
        }

        [Fact]
        public async Task DeleteGet_IdIsNull_NotFoundResult()
        {
            var controller = new StudentController(null);

            var result = await controller.Delete(null);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteGet_StudentIsNull_NotFoundResult()
        {
            int studentId = 1;
            IEnumerable<Student> nullGroup = null;
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
               It.IsAny<Expression<Func<Student, bool>>>(),
               It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
               It.IsAny<string>()))
                .ReturnsAsync(nullGroup);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.Delete(studentId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteGet_StudentId_ViewResultWithStudentDetails()
        {
            int studentId = 1;
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
               It.IsAny<Expression<Func<Student, bool>>>(),
               It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
               It.IsAny<string>()))
                .ReturnsAsync(studentList);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.Delete(studentId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Student>(viewResult.ViewData.Model);
            Assert.Equal(1, model.GroupId);
            Assert.Equal(1, model.StudentId);
            Assert.Equal("Ivan", model.FirstName);
            Assert.Equal("Ivanenko", model.LastName);
            Assert.IsType<Group>(model.Group);
        }

        [Fact]
        public async Task DeletePost_StudentRepositoryIsNull_RedirectedToIndex()
        {
            int studentId = 1;
            IEnumerable<Student> nullStudent = null;
            var mockRepo = new Mock<IUnitOfWork>();
            mockRepo.Setup(repo => repo.StudentsRepository).Returns((IGenericRepository<Student>)nullStudent);
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.DeleteConfirmed(studentId);

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            var problemResult = Assert.IsAssignableFrom<ProblemDetails>(objectResult.Value);
            Assert.Equal("Entity set 'EducationDBContext.Students'  is null.", problemResult.Detail);
        }

        [Fact]
        public async Task DeletePost_StudentToDelete_DeletedStudentAndRedirectedToIndex()
        {
            int studentId = 1;
            var student = studentList.First();
            var mockRepo = new Mock<IUnitOfWork>();

            mockRepo.Setup(repo => repo.StudentsRepository.GetAsync(
               It.IsAny<Expression<Func<Student, bool>>>(),
               It.IsAny<Func<IQueryable<Student>, IOrderedQueryable<Student>>>(),
               It.IsAny<string>()))
                .ReturnsAsync(studentList);

            mockRepo.Setup(repo => repo.StudentsRepository.Delete(student));
            mockRepo.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask).Verifiable();
            var controller = new StudentController(mockRepo.Object);

            var result = await controller.DeleteConfirmed(studentId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirectToActionResult.ControllerName);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mockRepo.Verify();
        }
    }
}

