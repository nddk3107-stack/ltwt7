using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CourseRegistration.Models;

namespace CourseRegistration.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Tạo Roles
        string[] roles = { "Admin", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Tạo tài khoản Admin
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Tạo tài khoản Student mẫu
        var studentEmail = "student@example.com";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);
        if (studentUser == null)
        {
            studentUser = new IdentityUser
            {
                UserName = "student",
                Email = studentEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(studentUser, "Student@123");
            await userManager.AddToRoleAsync(studentUser, "Student");
        }

        // Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Công nghệ thông tin" },
                new() { Name = "Kinh tế" },
                new() { Name = "Ngoại ngữ" },
                new() { Name = "Khoa học tự nhiên" }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Seed Courses
        if (!await context.Courses.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            var itCat = categories.First(c => c.Name == "Công nghệ thông tin");
            var econCat = categories.First(c => c.Name == "Kinh tế");
            var langCat = categories.First(c => c.Name == "Ngoại ngữ");
            var sciCat = categories.First(c => c.Name == "Khoa học tự nhiên");

            var courses = new List<Course>
            {
                new() { Name = "Lập trình Web", Credits = 3, Lecturer = "Nguyễn Văn A", CategoryId = itCat.Id, Image = "/images/courses/web.jpg" },
                new() { Name = "Cơ sở dữ liệu", Credits = 3, Lecturer = "Trần Thị B", CategoryId = itCat.Id, Image = "/images/courses/database.jpg" },
                new() { Name = "Trí tuệ nhân tạo", Credits = 4, Lecturer = "Lê Văn C", CategoryId = itCat.Id, Image = "/images/courses/ai.jpg" },
                new() { Name = "Mạng máy tính", Credits = 3, Lecturer = "Phạm Thị D", CategoryId = itCat.Id, Image = "/images/courses/network.jpg" },
                new() { Name = "Lập trình di động", Credits = 3, Lecturer = "Hoàng Văn E", CategoryId = itCat.Id, Image = "/images/courses/mobile.jpg" },
                new() { Name = "An toàn thông tin", Credits = 3, Lecturer = "Ngô Thị F", CategoryId = itCat.Id, Image = "/images/courses/security.jpg" },
                new() { Name = "Kinh tế vĩ mô", Credits = 3, Lecturer = "Đỗ Văn G", CategoryId = econCat.Id, Image = "/images/courses/economics.jpg" },
                new() { Name = "Quản trị kinh doanh", Credits = 4, Lecturer = "Vũ Thị H", CategoryId = econCat.Id, Image = "/images/courses/business.jpg" },
                new() { Name = "Tiếng Anh chuyên ngành", Credits = 2, Lecturer = "Bùi Văn I", CategoryId = langCat.Id, Image = "/images/courses/english.jpg" },
                new() { Name = "Toán cao cấp", Credits = 4, Lecturer = "Đinh Thị K", CategoryId = sciCat.Id, Image = "/images/courses/math.jpg" },
                new() { Name = "Vật lý đại cương", Credits = 3, Lecturer = "Trương Văn L", CategoryId = sciCat.Id, Image = "/images/courses/physics.jpg" },
                new() { Name = "Hóa học đại cương", Credits = 3, Lecturer = "Lý Thị M", CategoryId = sciCat.Id, Image = "/images/courses/chemistry.jpg" },
            };
            context.Courses.AddRange(courses);
            await context.SaveChangesAsync();
        }
    }
}
