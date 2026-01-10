using AutoMapper;
using BLL.DTOs.UserDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public AdminUserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = Log.ForContext<AdminUserService>();
        }

        public async Task<bool> CreateAdminUserAsync(CreateAdminUserDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                    throw new ArgumentException("Email is required", nameof(dto.Email));

                _logger.Information("Creating new admin user: {Email}", dto.Email);

                // Check if email already exists
                var existingUser = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    _logger.Warning("Admin user creation failed - email already exists: {Email}", dto.Email);
                    return false;
                }

                var user = new ApplicationUser
                {
                    Email = dto.Email,
                    UserName = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _unitOfWork.UserManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Error("Failed to create admin user: {Errors}", errors);
                    return false;
                }

                // Assign Admin role
                var roleResult = await _unitOfWork.UserManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    _logger.Error("Failed to assign Admin role: {Errors}", errors);
                    return false;
                }

                _logger.Information("Admin user created successfully: {Email}", dto.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating admin user: {Email}", dto.Email);
                return false;
            }
        }

        public async Task<bool> UpdateAdminUserAsync(string userId, UpdateAdminUserDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentException("User ID cannot be empty", nameof(userId));

                _logger.Information("Updating admin user: {UserId}", userId);

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("Admin user not found: {UserId}", userId);
                    return false;
                }

                // Verify user is admin
                var isAdmin = await _unitOfWork.UserManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin)
                {
                    _logger.Warning("User is not an admin: {UserId}", userId);
                    return false;
                }

                // Update fields if provided
                if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
                {
                    var emailExists = await _unitOfWork.UserManager.FindByEmailAsync(dto.Email);
                    if (emailExists != null)
                    {
                        _logger.Warning("Email already in use: {Email}", dto.Email);
                        return false;
                    }

                    user.Email = dto.Email;
                    user.UserName = dto.Email;
                }

                if (!string.IsNullOrWhiteSpace(dto.FullName))
                    user.FullName = dto.FullName;

                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    user.PhoneNumber = dto.PhoneNumber;

                if (dto.IsActive.HasValue)
                    user.IsActive = dto.IsActive.Value;

                var result = await _unitOfWork.UserManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Error("Failed to update admin user: {Errors}", errors);
                    return false;
                }

                _logger.Information("Admin user updated successfully: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating admin user: {UserId}", userId);
                return false;
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllAdminsAsync()
        {
            try
            {
                _logger.Debug("Retrieving all admin users");

                var admins = await _unitOfWork.UserManager.GetUsersInRoleAsync("Admin");

                var adminDtos = admins.Select(admin =>
                {
                    var dto = _mapper.Map<UserDto>(admin);
                    dto.Roles = new List<string> { "Admin" };
                    return dto;
                }).ToList();

                _logger.Information("Retrieved {Count} admin users", adminDtos.Count);
                return adminDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving admin users");
                throw;
            }
        }

        public async Task<UserDto?> GetAdminByIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return null;

                _logger.Debug("Retrieving admin user: {UserId}", userId);

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("Admin user not found: {UserId}", userId);
                    return null;
                }

                var isAdmin = await _unitOfWork.UserManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin)
                {
                    _logger.Warning("User is not an admin: {UserId}", userId);
                    return null;
                }

                var dto = _mapper.Map<UserDto>(user);
                dto.Roles = (await _unitOfWork.UserManager.GetRolesAsync(user)).ToList();

                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving admin user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteAdminAsync(string userId, string deletedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(deletedBy))
                    throw new ArgumentException("User ID and deletedBy cannot be empty");

                _logger.Warning("Deleting admin user: {UserId} by {DeletedBy}", userId, deletedBy);

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("Admin user not found: {UserId}", userId);
                    return false;
                }

                var result = await _unitOfWork.UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Error("Failed to delete admin user: {Errors}", errors);
                    return false;
                }

                _logger.Information("Admin user deleted: {UserId} by {DeletedBy}", userId, deletedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting admin user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeactivateAdminAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentException("User ID cannot be empty", nameof(userId));

                _logger.Information("Deactivating admin user: {UserId}", userId);

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("Admin user not found: {UserId}", userId);
                    return false;
                }

                user.IsActive = false;
                var result = await _unitOfWork.UserManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Error("Failed to deactivate admin user: {Errors}", errors);
                    return false;
                }

                _logger.Information("Admin user deactivated: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deactivating admin user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ReactivateAdminAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentException("User ID cannot be empty", nameof(userId));

                _logger.Information("Reactivating admin user: {UserId}", userId);

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("Admin user not found: {UserId}", userId);
                    return false;
                }

                user.IsActive = true;
                var result = await _unitOfWork.UserManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Error("Failed to reactivate admin user: {Errors}", errors);
                    return false;
                }

                _logger.Information("Admin user reactivated: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error reactivating admin user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ResetAdminPasswordAsync(string userId, string newPassword, string changedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newPassword))
                    throw new ArgumentException("User ID and new password cannot be empty");

                _logger.Information("Resetting password for admin user: {UserId} by {ChangedBy}", userId, changedBy);

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("Admin user not found: {UserId}", userId);
                    return false;
                }

                var token = await _unitOfWork.UserManager.GeneratePasswordResetTokenAsync(user);
                var result = await _unitOfWork.UserManager.ResetPasswordAsync(user, token, newPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.Error("Failed to reset admin password: {Errors}", errors);
                    return false;
                }

                _logger.Information("Admin user password reset: {UserId} by {ChangedBy}", userId, changedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error resetting admin password: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> IsAdminActiveAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var isAdmin = await _unitOfWork.UserManager.IsInRoleAsync(user, "Admin");
                return isAdmin && user.IsActive;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking if admin is active: {UserId}", userId);
                return false;
            }
        }

        public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetAdminsPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                    throw new ArgumentException("Page number and page size must be greater than 0");

                if (pageSize > 50)
                    pageSize = 50; // Max 50 items per page

                _logger.Debug("Retrieving admin users - Page {Page}, PageSize {PageSize}", pageNumber, pageSize);

                var allAdmins = await _unitOfWork.UserManager.GetUsersInRoleAsync("Admin");
                var totalCount = allAdmins.Count;

                var pagedAdmins = allAdmins
                    .OrderBy(a => a.FullName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var adminDtos = pagedAdmins.Select(admin =>
                {
                    var dto = _mapper.Map<UserDto>(admin);
                    dto.Roles = new List<string> { "Admin" };
                    return dto;
                }).ToList();

                _logger.Information("Retrieved {Count} of {Total} admin users", adminDtos.Count, totalCount);
                return (adminDtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving paged admin users");
                throw;
            }
        }
    }
}
