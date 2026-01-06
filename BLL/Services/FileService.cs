using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class FileService : IFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediaService _bunnyService;
        private readonly Serilog.ILogger _logger;

        public FileService(IUnitOfWork unitOfWork, IMediaService bunnyService)
        {
            _unitOfWork = unitOfWork;
            _bunnyService = bunnyService;
            _logger = Log.ForContext<FileService>();
        }

        public async Task<FileUploadDto> UploadVideoAsync(Stream fileStream, string fileName, string uploadedBy)
        {
            try
            {
                _logger.Information("Uploading video: {FileName} by user: {UserId}", fileName, uploadedBy);

                var cdnUrl = await _bunnyService.UploadVideoAsync(fileStream, fileName,uploadedBy);

                var fileMetadata = new FileMetadata
                {
                    FileName = fileName,
                    FileType = "Video",
                    FileSize = fileStream.Length,
                    BunnyCdnUrl = cdnUrl.Url,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow
                };

                await _unitOfWork.FileMetadatas.AddAsync(fileMetadata);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Video uploaded successfully: {FileName}", fileName);

                return new FileUploadDto
                {
                    FileName = fileName,
                    FileType = "Video",
                    FileSize = fileStream.Length,
                    Url = cdnUrl.Url
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error uploading video: {FileName}", fileName);
                throw;
            }
        }

        public async Task<FileUploadDto> UploadImageAsync(Stream fileStream, string fileName, string uploadedBy)
        {
            try
            {
                _logger.Information("Uploading image: {FileName} by user: {UserId}", fileName, uploadedBy);

                var cdnUrl = await _bunnyService.UploadImageAsync(fileStream, fileName, uploadedBy);

                var fileMetadata = new FileMetadata
                {
                    FileName = fileName,
                    FileType = "Image",
                    FileSize = fileStream.Length,
                    BunnyCdnUrl = cdnUrl.Url,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow
                };

                await _unitOfWork.FileMetadatas.AddAsync(fileMetadata);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Image uploaded successfully: {FileName}", fileName);

                return new FileUploadDto
                {
                    FileName = fileName,
                    FileType = "Image",
                    FileSize = fileStream.Length,
                    Url = cdnUrl.Url
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error uploading image: {FileName}", fileName);
                throw;
            }
        }

        public async Task<FileUploadDto> UploadPdfAsync(IFormFile fileStream, string fileName, string uploadedBy)
        {
            try
            {
                _logger.Information("Uploading PDF: {FileName} by user: {UserId}", fileName, uploadedBy);

                var cdnUrl = await _bunnyService.UploadFileAsync(fileStream, fileName);

                var fileMetadata = new FileMetadata
                {
                    FileName = fileName,
                    FileType = "PDF",
                    FileSize = fileStream.Length,
                    BunnyCdnUrl = cdnUrl,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow
                };

                await _unitOfWork.FileMetadatas.AddAsync(fileMetadata);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("PDF uploaded successfully: {FileName}", fileName);

                return new FileUploadDto
                {
                    FileName = fileName,
                    FileType = "PDF",
                    FileSize = fileStream.Length,
                    Url = cdnUrl
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error uploading PDF: {FileName}", fileName);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string url, string userId)
        {
            try
            {
                _logger.Information("Deleting file: {Url} by user: {UserId}", url, userId);

                var file = await _unitOfWork.FileMetadatas
                    .FindAsync(f => f.BunnyCdnUrl == url && !f.IsDeleted);

                if (file == null)
                {
                    _logger.Warning("File not found: {Url}", url);
                    return false;
                }

                // Extract path from URL for Bunny deletion
                var uri = new Uri(url);
                var path = uri.AbsolutePath.TrimStart('/');

                var deleted = await _bunnyService.DeleteFileAsync(path);

                if (deleted)
                {
                    file.IsDeleted = true;
                    file.DeletedAt = DateTime.UtcNow;
                    _unitOfWork.FileMetadatas.Update(file);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.Information("File deleted successfully: {Url}", url);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting file: {Url}", url);
                throw;
            }
        }

        public async Task<List<FileUploadDto>> GetUserFilesAsync(string userId)
        {
            try
            {
                _logger.Debug("Getting files for user: {UserId}", userId);

                var files = await _unitOfWork.FileMetadatas.GetFilesByUploaderAsync(userId);

                return files.Select(f => new FileUploadDto
                {
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileSize = f.FileSize,
                    Url = f.BunnyCdnUrl
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting user files");
                throw;
            }
        }
    }
}
