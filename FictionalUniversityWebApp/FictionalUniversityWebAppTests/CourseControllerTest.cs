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
	public class CourseControllerTest
	{
		public static List<Course> courseList = new()
		{
			new Course
			{
				 CourseId = 1,
				 Name = "Course One",
				 Description = "Desc 1",
				 Groups = new List<Group>
				 {
						new Group()
						{
							 GroupId = 1,
							 CourseId = 1,
							 Name = "SR-01"
						},
						new Group()
						{
							 GroupId = 2,
							 CourseId = 1,
							 Name = "SR-02"
						}
				 }
			},
			new Course
			{
				 CourseId = 2,
				 Name = "Course One",
				 Description = "Desc 1",
				 Groups = new List<Group>
				 {
						 new Group()
						 {
							  GroupId = 3,
							  CourseId = 2,
							  Name = "SR-03"
						 },
						 new Group()
						 {
							  GroupId = 4,
							  CourseId = 2,
							  Name = "SR-04"
						 }
				 }
			},
		};
		public static List<Course> emptycourseList = new()
		{
			new Course
			{
				 CourseId = 3,
				 Name = "Course Three",
				 Description = "Desc 3",
				 Groups = new List<Group>{}
			}
		};

		[Fact]
		public async Task Index_ViewResult_WithListOfCoursesOrderedByName()
		{
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetAsync(
				It.IsAny<Expression<Func<Course, bool>>>(),
				It.IsAny<Func<IQueryable<Course>, IOrderedQueryable<Course>>>(),
				It.IsAny<string>()))
					.ReturnsAsync(courseList);
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.Index();

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<IEnumerable<Course>>(viewResult.ViewData.Model);
			Assert.Equal(2, model.Count());
		}

		[Fact]
		public async Task Details_IdIsNull_NotFoundResult()
		{
			var controller = new CourseController(null);

			var result = await controller.Details(null);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task Details_CourseIsNull_NotFoundResult()
		{
			int courseId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetByIDAsync(courseId)).ReturnsAsync((Course)null);
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.Details(courseId);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task Details_CourseId_ViewResultWithCourseDetails()
		{
			int courseId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetByIDAsync(courseId))
				.ReturnsAsync(courseList.FirstOrDefault(c => c.CourseId == courseId));
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.Details(courseId);

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Course>(viewResult.ViewData.Model);
			Assert.Equal(1, model.CourseId);
			Assert.Equal("Course One", model.Name);
			Assert.Equal("Desc 1", model.Description);
			Assert.Equal(2, model.Groups.Count);
		}

		[Fact]
		public async Task CreatePost_InvalidModelState_ReturnsTheSameView()
		{
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetAsync(
				 It.IsAny<Expression<Func<Course, bool>>>(),
				 It.IsAny<Func<IQueryable<Course>, IOrderedQueryable<Course>>>(),
				 It.IsAny<string>()))
					 .ReturnsAsync(courseList);
			var controller = new CourseController(mockRepo.Object);
			controller.ModelState.AddModelError("SomeError", "Required");
			var newCourse = new Course();

			var result = await controller.Create(newCourse);
			var modelState = controller.ModelState;

			Assert.True(modelState.Keys.Contains("SomeError"));
			Assert.True(modelState["SomeError"].Errors.Count == 1);
			var invalidModelStateResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Course>(invalidModelStateResult.ViewData.Model);
		}

		[Fact]
		public async Task CreatePost_ValidModelState_AddedNewCourseAndRedirectedToIndex()
		{
			int courseId = 2;
			var newCourse = courseList.Find(c => c.CourseId == courseId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.Insert(newCourse));
			mockRepo.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask).Verifiable();
			var controller = new CourseController(mockRepo.Object);


			var result = await controller.Create(newCourse);

			var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Null(redirectToActionResult.ControllerName);
			Assert.Equal("Index", redirectToActionResult.ActionName);
			mockRepo.Verify();
		}

		[Fact]
		public async Task EditGet_IdIsNull_NotFoundResult()
		{
			var controller = new CourseController(null);

			var result = await controller.Edit(null);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task EditGet_CourseIsNull_NotFoundResult()
		{
			int courseId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetByIDAsync(courseId)).ReturnsAsync((Course)null);
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.Edit(courseId);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task EditGet_CourseId_ViewResultWithCourseDetails()
		{
			int courseId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetByIDAsync(courseId))
				.ReturnsAsync(courseList.FirstOrDefault(c => c.CourseId == courseId));
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.Edit(courseId);

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Course>(viewResult.ViewData.Model);
			Assert.Equal(1, model.CourseId);
			Assert.Equal("Course One", model.Name);
			Assert.Equal("Desc 1", model.Description);
			Assert.Equal(2, model.Groups.Count);
		}

		[Fact]
		public async Task EditPost_InvalidCourseId_NotFoundResult()
		{
			int courseId = 1;
			int nextCourseId = 2;
			var course = courseList.Find(c => c.CourseId == courseId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetByIDAsync(courseId))
				.ReturnsAsync(course);
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.Edit(nextCourseId, course);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task EditPost_InvalidModelState_ReturnsTheSameView()
		{
			int courseId = 1;
			var course = courseList.Find(c => c.CourseId == courseId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetByIDAsync(courseId))
				.ReturnsAsync(course);
			var controller = new CourseController(mockRepo.Object);
			controller.ModelState.AddModelError("SomeError", "Required");

			var result = await controller.Edit(courseId, course);
			var modelState = controller.ModelState;

			Assert.True(modelState.Keys.Contains("SomeError"));
			Assert.True(modelState["SomeError"].Errors.Count == 1);
			var invalidModelStateResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Course>(invalidModelStateResult.ViewData.Model);
		}

		[Fact]
		public async Task EditPost_ValidModelState_UpdatedCourseAndRedirectedToIndex()
		{
			int courseId = 2;
			var course = courseList.Find(c => c.CourseId == courseId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.Update(course));
			mockRepo.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask).Verifiable();
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.Edit(courseId, course);

			var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Null(redirectToActionResult.ControllerName);
			Assert.Equal("Index", redirectToActionResult.ActionName);
			mockRepo.Verify();
		}

		[Fact]
		public async Task EditPost_ValidModelStateButDbUpdateConcurrencyExceptionAndCourseExists_ThrownException()
		{
			int courseId = 2;
			var course = courseList.Find(c => c.CourseId == courseId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetAsync(
				It.IsAny<Expression<Func<Course, bool>>>(),
				It.IsAny<Func<IQueryable<Course>, IOrderedQueryable<Course>>>(),
				It.IsAny<string>()))
					.ReturnsAsync(courseList);
			mockRepo.Setup(repo => repo.CoursesRepository.Update(course))
				.Throws(new DbUpdateConcurrencyException(String.Empty, new List<IUpdateEntry> { Mock.Of<IUpdateEntry>() }));
			var controller = new CourseController(mockRepo.Object);

			Func<Task> act = () => controller.Edit(courseId, course);

			var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(act);
		}

		[Fact]
		public async Task DeleteGet_IdIsNull_NotFoundResult()
		{
			var controller = new CourseController(null);

			var result = await controller.Delete(null);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task DeleteGet_CourseIsNull_NotFoundResult()
		{
			int courseId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetByIDAsync(courseId)).ReturnsAsync((Course)null);
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.Delete(courseId);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task DeleteGet_CourseId_ViewResultWithCourseDetails()
		{
			int courseId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository.GetByIDAsync(courseId))
				.ReturnsAsync(courseList.FirstOrDefault(c => c.CourseId == courseId));
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.Delete(courseId);

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Course>(viewResult.ViewData.Model);
			Assert.Equal(1, model.CourseId);
			Assert.Equal("Course One", model.Name);
			Assert.Equal("Desc 1", model.Description);
			Assert.Equal(2, model.Groups.Count);
		}

		[Fact]
		public async Task DeletePost_CourseRepositoryIsNull_RedirectedToIndex()
		{
			int courseId = 1;
			IEnumerable<Course> nullCourse = null;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.CoursesRepository).Returns((IGenericRepository<Course>)nullCourse);
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.DeleteConfirmed(courseId);

			var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
			var problemResult = Assert.IsAssignableFrom<ProblemDetails>(objectResult.Value);
			Assert.Equal("Entity set 'EducationDBContext.Courses'  is null.", problemResult.Detail);
		}

		[Fact]
		public async Task DeletePost_NotEmptyCourseToDelete_ModelStateErrorAndTheSameView()
		{
			int courseId = 2;
			var course = courseList.Find(c => c.CourseId == courseId);
			var mockRepo = new Mock<IUnitOfWork>();

			mockRepo.Setup(repo => repo.CoursesRepository.GetAsync(
			   It.IsAny<Expression<Func<Course, bool>>>(),
			   It.IsAny<Func<IQueryable<Course>, IOrderedQueryable<Course>>>(),
			   It.IsAny<string>()))
				.ReturnsAsync(courseList);
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.DeleteConfirmed(courseId);
			var modelState = controller.ModelState;

			Assert.True(modelState.Keys.Contains("NotEmptyCourse"));
			Assert.True(modelState["NotEmptyCourse"].Errors.Count == 1);
			var invalidModelStateResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Course>(invalidModelStateResult.ViewData.Model);
			mockRepo.Verify(repo => repo.SaveAsync(), Times.Never);
		}

		[Fact]
		public async Task DeletePost_EmptyCourseToDelete_DeletedCourseAndRedirectedToIndex()
		{
			int emptyCourseId = 3;
			var emptyCourse = emptycourseList.First(c => c.CourseId == emptyCourseId);
			var mockRepo = new Mock<IUnitOfWork>();

			mockRepo.Setup(repo => repo.CoursesRepository.GetAsync(
			   It.IsAny<Expression<Func<Course, bool>>>(),
			   It.IsAny<Func<IQueryable<Course>, IOrderedQueryable<Course>>>(),
			   It.IsAny<string>()))
				.ReturnsAsync(emptycourseList);

			mockRepo.Setup(repo => repo.CoursesRepository.Delete(emptyCourse));
			mockRepo.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask).Verifiable();
			var controller = new CourseController(mockRepo.Object);

			var result = await controller.DeleteConfirmed(emptyCourseId);

			var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Null(redirectToActionResult.ControllerName);
			Assert.Equal("Index", redirectToActionResult.ActionName);
			mockRepo.Verify();
		}
	}
}