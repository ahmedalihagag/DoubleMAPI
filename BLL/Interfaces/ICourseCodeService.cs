using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICourseCodeService
    {
        Task<string> GenerateCourseCodeAsync(int courseId, string adminId, int expiryDays = 30);
        Task<List<CourseCodeDto>> GetCourseCodesAsync(int courseId);
        Task<bool> ValidateCourseCodeAsync(string code);
        Task<bool> RevokeCourseCodeAsync(string code, string adminId);
    }
}
