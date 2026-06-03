using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseRegistration.Data;
using CourseRegistration.Models;
using System.Security.Claims;

namespace CourseRegistration.Controllers;

// Câu 4: /enroll/** chỉ STUDENT
// Câu 6: Enroll/Unenroll Course
// Câu 7: My Courses
[Authorize(Roles = "Student")]
[Route("enroll/[action]")]
public class EnrollmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public EnrollmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Câu 6: Đăng ký học phần
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        // Kiểm tra đã đăng ký chưa
        var exists = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (!exists)
        {
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrollDate = DateTime.Now
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đăng ký học phần thành công!";
        }
        else
        {
            TempData["Warning"] = "Bạn đã đăng ký học phần này rồi.";
        }

        return RedirectToAction("Index", "Home");
    }

    // Câu 6: Hủy đăng ký
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int courseId, string? returnUrl = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (enrollment != null)
        {
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hủy đăng ký thành công!";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    // Câu 7: My Courses
    [Route("/enroll/mycourses")]
    public async Task<IActionResult> MyCourses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c!.Category)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrollDate)
            .ToListAsync();

        return View(enrollments);
    }
}
