using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FictionalUniversityWebApp.Models;
using FictionalUniversityWebApp.Properties;
using FictionalUniversityWebApp.DAL.Interfaces;

namespace FictionalUniversityWebApp.Controllers
{
    public class CourseController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public CourseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _unitOfWork.CoursesRepository.GetAsync(orderBy: q => q.OrderBy(c => c.Name));
            return View(courses);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _unitOfWork.CoursesRepository == null)
            {
                return NotFound();
            }
            var course = await _unitOfWork.CoursesRepository.GetByIDAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,Name,Description")] Course course)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoursesRepository.Insert(course);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _unitOfWork.CoursesRepository == null)
            {
                return NotFound();
            }
            var course = await _unitOfWork.CoursesRepository.GetByIDAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,Name,Description")] Course course)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.CoursesRepository.Update(course);
                    await _unitOfWork.SaveAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId))
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
            return View(course);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _unitOfWork.CoursesRepository == null)
            {
                return NotFound();
            }
            var course = await _unitOfWork.CoursesRepository.GetByIDAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_unitOfWork.CoursesRepository == null)
            {
                return Problem(Resources.CoursesNull);
            }
            var course = await _unitOfWork.CoursesRepository.GetByIDAsync(id);
            if (course != null)
            {
                _unitOfWork.CoursesRepository.Delete(course);
            }
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            var courses = _unitOfWork.CoursesRepository.GetAsync();
            return courses.Result.Any(e => e.CourseId == id);
        }
    }
}
