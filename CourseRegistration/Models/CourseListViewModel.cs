namespace CourseRegistration.Models;

public class CourseListViewModel
{
    public List<Course> Courses { get; set; } = new();
    public string? SearchQuery { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 5;

    // Danh sách CourseId mà user đã enroll
    public List<int> EnrolledCourseIds { get; set; } = new();
    public bool IsStudent { get; set; }
    public bool IsAuthenticated { get; set; }
}
