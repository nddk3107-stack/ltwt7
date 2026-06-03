using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CourseRegistration.Models;

public class Enrollment
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int CourseId { get; set; }

    [Display(Name = "Ngày đăng ký")]
    public DateTime EnrollDate { get; set; } = DateTime.Now;

    public IdentityUser? User { get; set; }
    public Course? Course { get; set; }
}
