using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FictionalUniversityWebApp.Properties;
using FictionalUniversityWebApp.DAL.Interfaces;

namespace FictionalUniversityWebApp.Controllers
{
	public class GroupController : Controller
	{
		private const int MinStudentsToDeleteGroup = 1;
		private IUnitOfWork _unitOfWork;

		public GroupController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IActionResult> Index()
		{
			var groups = await _unitOfWork.GroupsRepository.GetAsync(
				orderBy: q => q.OrderBy(g => g.Name),
				includeProperties: "Course");

			return View(groups);
		}

		public async Task<IActionResult> GroupSelection(int? id)
		{
			if (id == null || _unitOfWork.GroupsRepository == null)
			{
				return NotFound();
			}
			var groups = await _unitOfWork.GroupsRepository.GetAsync(
				filter: q => q.CourseId == id,
				orderBy: q => q.OrderBy(g => g.Name),
				includeProperties: "Course");

			return View(groups);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _unitOfWork.GroupsRepository == null)
			{
				return NotFound();
			}
			var group = await _unitOfWork.GroupsRepository.GetAsync(
				filter: q => q.GroupId == id,
				includeProperties: "Course");

			if (group == null)
			{
				return NotFound();
			}
			return View(group.FirstOrDefault());
		}

		public IActionResult Create()
		{
			ViewData["CourseId"] = new SelectList(_unitOfWork.CoursesRepository.GetAsync().Result, "CourseId", "Name");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("GroupId,CourseId,Name")] Models.Group group)
		{
			if (ModelState.IsValid)
			{
				_unitOfWork.GroupsRepository.Insert(group);
				await _unitOfWork.SaveAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["CourseId"] = new SelectList(_unitOfWork.CoursesRepository.GetAsync().Result, "CourseId", "Name", group.CourseId);
			return View(group);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _unitOfWork.GroupsRepository == null)
			{
				return NotFound();
			}
			var group = await _unitOfWork.GroupsRepository.GetAsync(
				filter: q => q.GroupId == id,
				includeProperties: "Course");

			if (group == null)
			{
				return NotFound();
			}
			return View(group.FirstOrDefault());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("GroupId,CourseId,Name")] Models.Group group)
		{
			if (id != group.GroupId)
			{
				return NotFound();
			}
			if (ModelState.IsValid)
			{
				try
				{
					_unitOfWork.GroupsRepository.Update(group);
					await _unitOfWork.SaveAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!GroupExists(group.GroupId))
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
			return View(group);
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _unitOfWork.GroupsRepository == null)
			{
				return NotFound();
			}
			var group = await _unitOfWork.GroupsRepository.GetAsync(
				filter: q => q.GroupId == id,
				includeProperties: "Course");

			if (group == null)
			{
				return NotFound();
			}
			return View(group.FirstOrDefault());
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_unitOfWork.GroupsRepository == null)
			{
				return Problem(Resources.GroupsNull);
			}
			var group = await _unitOfWork.GroupsRepository.GetAsync(
				filter: q => q.GroupId == id,
				includeProperties: "Course,Students");

			if (group.FirstOrDefault().Students.Count >= MinStudentsToDeleteGroup)
			{
				ModelState.AddModelError("NotEmptyGroup", Resources.NotEmptyGroupMessage);
				return View(group.FirstOrDefault());
			}
			if (group.FirstOrDefault() != null)
			{
				_unitOfWork.GroupsRepository.Delete(group.FirstOrDefault());
			}

			await _unitOfWork.SaveAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool GroupExists(int id)
		{
			var groups = _unitOfWork.GroupsRepository.GetAsync();
			return groups.Result.Any(e => e.GroupId == id);
		}
	}
}
