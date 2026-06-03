using System.Globalization;
using System.Security.Claims;
using System.Text;
using CourseRegistration.Data;
using CourseRegistration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseRegistration.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 5;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/")]
    [HttpGet("/home")]
    [HttpGet("/courses")]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        search = search?.Trim();
        page = Math.Max(page, 1);

        var allCourses = await _context.Courses
            .Include(c => c.Category)
            .OrderBy(c => c.Name)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = NormalizeSearchText(search);
            allCourses = allCourses
                .Where(c => NormalizeSearchText(c.Name).Contains(keyword))
                .ToList();
        }

        var totalCourses = allCourses.Count;
        var totalPages = (int)Math.Ceiling(totalCourses / (double)PageSize);
        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var courses = allCourses
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        var viewModel = new CourseListViewModel
        {
            Courses = courses,
            SearchQuery = search,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = PageSize,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            IsStudent = User.IsInRole("Student")
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            viewModel.EnrolledCourseIds = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();
        }

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    private static string NormalizeSearchText(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
