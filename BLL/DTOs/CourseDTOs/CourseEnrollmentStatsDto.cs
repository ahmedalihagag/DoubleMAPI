namespace BLL.DTOs.CourseDTOs;

/// <summary>
/// ✅ Statistics for course enrollment and codes
/// </summary>
public class CourseEnrollmentStatsDto
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public int TotalEnrolled { get; set; }

    public int ActiveCodesCount { get; set; }

    public DateTime UpdatedAt { get; set; }
}
