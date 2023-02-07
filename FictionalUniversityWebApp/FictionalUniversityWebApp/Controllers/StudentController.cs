using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FictionalUniversityWebApp.Models;
using FictionalUniversityWebApp.Properties;
using FictionalUniversityWebApp.DAL.Interfaces;

namespace FictionalUniversityWebApp.Controllers
{
    public class StudentController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public StudentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _unitOfWork.StudentsRepository.GetAsync(
                orderBy: q => q.OrderBy(s => s.LastName),
                includeProperties: "Group");

            return View(students);
        }

        public async Task<IActionResult> StudentSelection(int? id)
        {
            if (id == null || _unitOfWork.StudentsRepository == null)
            {
                return NotFound();
            }
            var students = await _unitOfWork.StudentsRepository.GetAsync(
                filter: q => q.GroupId == id,
                orderBy: q => q.OrderBy(s => s.LastName),
                includeProperties: "Group");

            return View(students);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _unitOfWork.StudentsRepository == null)
            {
                return NotFound();
            }
            var student = await _unitOfWork.StudentsRepository.GetAsync(
                filter: q => q.StudentId == id,
                includeProperties: "Group");

            if (student == null)
            {
                return NotFound();
            }
            return View(student.FirstOrDefault());
        }

        public IActionResult Create()
        {
            ViewData["GroupId"] = new SelectList(_unitOfWork.GroupsRepository.GetAsync().Result, "GroupId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,GroupId,FirstName,LastName")] Student student)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.StudentsRepository.Insert(student);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GroupId"] = new SelectList(_unitOfWork.GroupsRepository.GetAsync().Result, "GroupId", "Name", student.GroupId);
            return View(student);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _unitOfWork.StudentsRepository == null)
            {
                return NotFound();
            }
            var student = await _unitOfWork.StudentsRepository.GetAsync(
                filter: q => q.StudentId == id,
                includeProperties: "Group");

            if (student == null)
            {
                return NotFound();
            }
            return View(student.FirstOrDefault());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StudentId,GroupId,FirstName,LastName")] Student student)
        {
            if (id != student.StudentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.StudentsRepository.Update(student);
                    await _unitOfWork.SaveAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.StudentId))
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
            return View(student);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _unitOfWork.StudentsRepository == null)
            {
                return NotFound();
            }
            var student = await _unitOfWork.StudentsRepository.GetAsync(
                filter: q => q.StudentId == id,
                includeProperties: "Group");

            if (student == null)
            {
                return NotFound();
            }
            return View(student.FirstOrDefault());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_unitOfWork.StudentsRepository == null)
            {
                return Problem(Resources.StudentsNull);
            }
            var student = await _unitOfWork.StudentsRepository.GetByIDAsync(id);
            if (student != null)
            {
                _unitOfWork.StudentsRepository.Delete(student);
            }
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            var students = _unitOfWork.StudentsRepository.GetAsync();
            return students.Result.Any(e => e.StudentId == id);
        }
    }
}
