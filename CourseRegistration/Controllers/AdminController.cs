using CourseRegistration.Data;
using CourseRegistration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CourseRegistration.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/[action]")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [Route("/admin")]
    public async Task<IActionResult> Index(string? search)
    {
        search = search?.Trim();
        ViewData["Search"] = search;

        var query = _context.Courses
            .Include(c => c.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(keyword) ||
                c.Lecturer.ToLower().Contains(keyword) ||
                (c.Category != null && c.Category.Name.ToLower().Contains(keyword)));
        }

        var courses = await query.OrderBy(c => c.Name).ToListAsync();

        return View(courses);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(course.CategoryId);
            return View(course);
        }

        if (imageFile is { Length: > 0 })
        {
            course.Image = await SaveImageAsync(imageFile);
        }

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Thêm học phần thành công!";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        await LoadCategoriesAsync(course.CategoryId);
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course course, IFormFile? imageFile)
    {
        if (id != course.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(course.CategoryId);
            return View(course);
        }

        var existing = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (existing == null)
        {
            return NotFound();
        }

        course.Image = imageFile is { Length: > 0 }
            ? await SaveImageAsync(imageFile)
            : existing.Image;

        _context.Update(course);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Cập nhật học phần thành công!";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Id == id);

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
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa học phần thành công!";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync(int? selectedCategoryId = null)
    {
        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "images", "courses");
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/courses/{fileName}";
    }
}
