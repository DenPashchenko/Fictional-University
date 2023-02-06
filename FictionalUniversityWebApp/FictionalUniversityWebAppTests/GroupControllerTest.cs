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
	public class GroupControllerTest
	{
		public static List<Group> groupList = new()
		{
			new Group
			{
				GroupId = 1,
				CourseId = 1,
				Name = "SR-01",
				Course = new Course()
				{
					CourseId = 1,
					Name = "Course One",
					Description = "Desc 1"
				},
				Students = new List<Student>
				{
						new Student()
						{
							StudentId = 1,
							GroupId = 1,
							FirstName = "Ivan",
							LastName = "Ivanenko"
						},
						new Student()
						{
							StudentId = 2,
							GroupId = 1,
							FirstName = "Petro",
							LastName = "Ivanenko"
						}
				}
			},
			new Group
			{
				GroupId = 2,
				CourseId = 1,
				Name = "SR-02",
				Course = new Course(),
				Students = new List<Student>
				{
						new Student()
						{
							StudentId = 3,
							GroupId = 2,
							FirstName = "Roman",
							LastName = "Petrenko"
						},
						new Student()
						{
							StudentId = 4,
							GroupId = 2,
							FirstName = "Petro",
							LastName = "Romanenko"
						}
				}
			}
		};
		public static List<Group> emptygroupList = new()
		{
			new Group
			{
				GroupId = 1,
				CourseId = 2,
				Name = "SR-03",
				Course = new Course(),
				Students = new List<Student>{}
			}
		};

		[Fact]
		public async Task Index_ViewResult_WithListOfGroupsOrderedByName()
		{
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
				It.IsAny<Expression<Func<Group, bool>>>(),
				It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
				It.IsAny<string>()))
					.ReturnsAsync(groupList);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.Index();

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<IEnumerable<Group>>(viewResult.ViewData.Model);
			Assert.Equal(2, model.Count());
		}

		[Fact]
		public async Task GroupSelection_IdIsNull_NotFoundResult()
		{
			var controller = new GroupController(null);

			var result = await controller.GroupSelection(null);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task GroupSelection_GroupRepositoryIsNull_NotFoundResult()
		{
			int groupId = 1;
			IEnumerable<Group> nullGroup = null;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository).Returns((IGenericRepository<Group>)nullGroup);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.GroupSelection(groupId);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task GroupSelection_CourseId_ViewResultWithGroupsInCourse()
		{
			int courseId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
				It.IsAny<Expression<Func<Group, bool>>>(),
				It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
				It.IsAny<string>()))
					.ReturnsAsync(groupList);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.GroupSelection(courseId);

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<IEnumerable<Group>>(viewResult.ViewData.Model);
			Assert.Equal(2, model.Count());
		}

		[Fact]
		public async Task Details_IdIsNull_NotFoundResult()
		{
			var controller = new GroupController(null);

			var result = await controller.Details(null);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task Details_GroupIsNull_NotFoundResult()
		{
			int groupId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
				It.IsAny<Expression<Func<Group, bool>>>(),
				It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
				It.IsAny<string>()))
					.ReturnsAsync((IEnumerable<Group>)null);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.Details(groupId);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task Details_GroupId_ViewResultWithGroupDetails()
		{
			int groupId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
				It.IsAny<Expression<Func<Group, bool>>>(),
				It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
				It.IsAny<string>()))
					.ReturnsAsync(groupList);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.Details(groupId);

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Group>(viewResult.ViewData.Model);
			Assert.Equal(1, model.GroupId);
			Assert.Equal(1, model.CourseId);
			Assert.Equal("SR-01", model.Name);
			Assert.IsType<Course>(model.Course);
			Assert.Equal(2, model.Students.Count);
		}

		[Fact]
		public async Task CreatePost_InvalidModelState_ReturnsTheSameView()
		{
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
				 It.IsAny<Expression<Func<Group, bool>>>(),
				 It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
				 It.IsAny<string>()))
					 .ReturnsAsync(groupList);
			mockRepo.Setup(repo => repo.CoursesRepository.GetAsync(
				It.IsAny<Expression<Func<Course, bool>>>(),
				It.IsAny<Func<IQueryable<Course>, IOrderedQueryable<Course>>>(),
				It.IsAny<string>()))
					.ReturnsAsync(CourseControllerTest.courseList);
			var controller = new GroupController(mockRepo.Object);
			controller.ModelState.AddModelError("SomeError", "Required");
			var newGroup = new Group();

			var result = await controller.Create(newGroup);
			var modelState = controller.ModelState;

			Assert.True(modelState.Keys.Contains("SomeError"));
			Assert.True(modelState["SomeError"].Errors.Count == 1);
			var invalidModelStateResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Group>(invalidModelStateResult.ViewData.Model);
		}

		[Fact]
		public async Task CreatePost_ValidModelState_AddedNewGroupAndRedirectedToIndex()
		{
			int groupId = 2;
			var newGroup = groupList.Find(g => g.GroupId == groupId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.Insert(newGroup));
			mockRepo.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask).Verifiable();
			var controller = new GroupController(mockRepo.Object);


			var result = await controller.Create(newGroup);

			var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Null(redirectToActionResult.ControllerName);
			Assert.Equal("Index", redirectToActionResult.ActionName);
			mockRepo.Verify();
		}

		[Fact]
		public async Task EditGet_IdIsNull_NotFoundResult()
		{
			var controller = new GroupController(null);

			var result = await controller.Edit(null);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task EditGet_GroupIsNull_NotFoundResult()
		{
			int groupId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
			   It.IsAny<Expression<Func<Group, bool>>>(),
			   It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
			   It.IsAny<string>())).ReturnsAsync((IEnumerable<Group>)null);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.Edit(groupId);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task EditGet_GroupId_ViewResultWithGroupDetails()
		{
			int groupId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
			   It.IsAny<Expression<Func<Group, bool>>>(),
			   It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
			   It.IsAny<string>()))
				.ReturnsAsync(groupList);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.Edit(groupId);

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Group>(viewResult.ViewData.Model);
			Assert.IsType<Course>(model.Course);
			Assert.Equal(1, model.CourseId);
			Assert.Equal("SR-01", model.Name);
			Assert.Equal(2, model.Students.Count);
		}

		[Fact]
		public async Task EditPost_InvalidGroupId_NotFoundResult()
		{
			int groupId = 1;
			int nextGroupId = 2;
			var group = groupList.Find(g => g.GroupId == groupId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
			   It.IsAny<Expression<Func<Group, bool>>>(),
			   It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
			   It.IsAny<string>()))
				.ReturnsAsync(groupList);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.Edit(nextGroupId, group);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task EditPost_InvalidModelState_ReturnsTheSameView()
		{
			int groupId = 1;
			var group = groupList.Find(c => c.CourseId == groupId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
			   It.IsAny<Expression<Func<Group, bool>>>(),
			   It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
			   It.IsAny<string>()))
				.ReturnsAsync(groupList);
			var controller = new GroupController(mockRepo.Object);
			controller.ModelState.AddModelError("SomeError", "Required");

			var result = await controller.Edit(groupId, group);
			var modelState = controller.ModelState;

			Assert.True(modelState.Keys.Contains("SomeError"));
			Assert.True(modelState["SomeError"].Errors.Count == 1);
			var invalidModelStateResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Group>(invalidModelStateResult.ViewData.Model);
		}

		[Fact]
		public async Task EditPost_ValidModelState_UpdatedGroupAndRedirectedToIndex()
		{
			int groupId = 2;
			var group = groupList.Find(g => g.GroupId == groupId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.Update(group));
			mockRepo.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask).Verifiable();
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.Edit(groupId, group);

			var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Null(redirectToActionResult.ControllerName);
			Assert.Equal("Index", redirectToActionResult.ActionName);
			mockRepo.Verify();
		}

		[Fact]
		public async Task EditPost_ValidModelStateButDbUpdateConcurrencyExceptionAndGroupExists_ThrownException()
		{
			int groupId = 2;
			var group = groupList.Find(g => g.GroupId == groupId);
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
				It.IsAny<Expression<Func<Group, bool>>>(),
				It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
				It.IsAny<string>()))
					.ReturnsAsync(groupList);
			mockRepo.Setup(repo => repo.GroupsRepository.Update(group))
				.Throws(new DbUpdateConcurrencyException(String.Empty, new List<IUpdateEntry> { Mock.Of<IUpdateEntry>() }));
			var controller = new GroupController(mockRepo.Object);

			Func<Task> act = () => controller.Edit(groupId, group);

			var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(act);
		}

		[Fact]
		public async Task DeleteGet_IdIsNull_NotFoundResult()
		{
			var controller = new GroupController(null);

			var result = await controller.Delete(null);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task DeleteGet_GroupIsNull_NotFoundResult()
		{
			int groupId = 1;
			IEnumerable<Group> nullGroup = null;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
			   It.IsAny<Expression<Func<Group, bool>>>(),
			   It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
			   It.IsAny<string>()))
				.ReturnsAsync(nullGroup);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.Delete(groupId);

			var notFoundResult = Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task DeleteGet_GroupId_ViewResultWithGroupDetails()
		{
			int groupId = 1;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
			   It.IsAny<Expression<Func<Group, bool>>>(),
			   It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
			   It.IsAny<string>()))
				.ReturnsAsync(groupList);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.Delete(groupId);

			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Group>(viewResult.ViewData.Model);
			Assert.IsType<Course>(model.Course);
			Assert.Equal(1, model.CourseId);
			Assert.Equal("SR-01", model.Name);
			Assert.Equal(2, model.Students.Count);
		}

		[Fact]
		public async Task DeletePost_GroupRepositoryIsNull_RedirectedToIndex()
		{
			int groupId = 1;
			IEnumerable<Group> nullGroup = null;
			var mockRepo = new Mock<IUnitOfWork>();
			mockRepo.Setup(repo => repo.GroupsRepository).Returns((IGenericRepository<Group>)nullGroup);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.DeleteConfirmed(groupId);

			var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
			var problemResult = Assert.IsAssignableFrom<ProblemDetails>(objectResult.Value);
			Assert.Equal("Entity set 'EducationDBContext.Groups'  is null.", problemResult.Detail);
		}

		[Fact]
		public async Task DeletePost_NotEmptyGroupToDelete_ModelStateErrorAndTheSameView()
		{
			int groupId = 2;
			var group = groupList.Find(g => g.GroupId == groupId);
			var mockRepo = new Mock<IUnitOfWork>();

			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
			   It.IsAny<Expression<Func<Group, bool>>>(),
			   It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
			   It.IsAny<string>()))
				.ReturnsAsync(groupList);
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.DeleteConfirmed(groupId);
			var modelState = controller.ModelState;

			Assert.True(modelState.Keys.Contains("NotEmptyGroup"));
			Assert.True(modelState["NotEmptyGroup"].Errors.Count == 1);
			var invalidModelStateResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsType<Group>(invalidModelStateResult.ViewData.Model);
			mockRepo.Verify(repo => repo.SaveAsync(), Times.Never);
		}

		[Fact]
		public async Task DeletePost_EmptyGroupToDelete_DeletedGroupAndRedirectedToIndex()
		{
			int emptyGroupId = 1;
			var emptyGroup = emptygroupList.First();
			var mockRepo = new Mock<IUnitOfWork>();

			mockRepo.Setup(repo => repo.GroupsRepository.GetAsync(
			   It.IsAny<Expression<Func<Group, bool>>>(),
			   It.IsAny<Func<IQueryable<Group>, IOrderedQueryable<Group>>>(),
			   It.IsAny<string>()))
				.ReturnsAsync(emptygroupList);

			mockRepo.Setup(repo => repo.GroupsRepository.Delete(emptyGroup));
			mockRepo.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask).Verifiable();
			var controller = new GroupController(mockRepo.Object);

			var result = await controller.DeleteConfirmed(emptyGroupId);

			var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
			Assert.Null(redirectToActionResult.ControllerName);
			Assert.Equal("Index", redirectToActionResult.ActionName);
			mockRepo.Verify();
		}
	}
}
