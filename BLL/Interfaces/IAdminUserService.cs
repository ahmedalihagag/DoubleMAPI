using BLL.DTOs.UserDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAdminUserService
    {
        /// <summary>
        /// Create a new admin user
        /// </summary>
        Task<bool> CreateAdminUserAsync(CreateAdminUserDto dto);

        /// <summary>
        /// Update existing admin user information
        /// </summary>
        Task<bool> UpdateAdminUserAsync(string userId, UpdateAdminUserDto dto);

        /// <summary>
        /// Get all admin users
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllAdminsAsync();

        /// <summary>
        /// Get admin user by ID
        /// </summary>
        Task<UserDto?> GetAdminByIdAsync(string userId);

        /// <summary>
        /// Delete admin user
        /// </summary>
        Task<bool> DeleteAdminAsync(string userId, string deletedBy);

        /// <summary>
        /// Deactivate admin user without deletion
        /// </summary>
        Task<bool> DeactivateAdminAsync(string userId);

        /// <summary>
        /// Reactivate previously deactivated admin user
        /// </summary>
        Task<bool> ReactivateAdminAsync(string userId);

        /// <summary>
        /// Reset admin user password
        /// </summary>
        Task<bool> ResetAdminPasswordAsync(string userId, string newPassword, string changedBy);

        /// <summary>
        /// Verify admin user exists and is active
        /// </summary>
        Task<bool> IsAdminActiveAsync(string userId);

        /// <summary>
        /// Get admin users with pagination
        /// </summary>
        Task<(IEnumerable<UserDto> Users, int TotalCount)> GetAdminsPagedAsync(int pageNumber, int pageSize);
    }
}
