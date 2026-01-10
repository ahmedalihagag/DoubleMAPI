namespace BLL.DTOs.CourseDTOs;

public class CourseEnrollmentStatsDto
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public int TotalEnrolled { get; set; }

    public int ActiveCodesCount { get; set; }

    public DateTime UpdatedAt { get; set; }
}
