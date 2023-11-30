namespace Cursus.DTO.Course;

public class CourseListDTO
{
    public List<CourseDTO> List { get; set; }
    public int Total { get; set; }
    public string? SortBy { get; set; }
}