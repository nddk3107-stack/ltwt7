using System.ComponentModel.DataAnnotations;

namespace CourseRegistration.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Hình ảnh")]
    public string? Image { get; set; }

    [Required]
    [Range(1, 10)]
    [Display(Name = "Số tín chỉ")]
    public int Credits { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Giảng viên")]
    public string Lecturer { get; set; } = string.Empty;

    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
