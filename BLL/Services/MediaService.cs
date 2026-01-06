using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class MediaService : IMediaService
    {
        private readonly ILogger<MediaService> _logger;
        private readonly BunnyCDNStorage _bunnyStorage;
        private readonly IUnitOfWork _unitOfWork;

        public MediaService(ILogger<MediaService> logger, BunnyCDNStorage bunnyStorage, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _bunnyStorage = bunnyStorage;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            try
            {
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(folder, fileName);

                using var stream = file.OpenReadStream();
                await _bunnyStorage.UploadAsync(path, stream);

                _logger.LogInformation("File uploaded: {Path}", path);
                return _bunnyStorage.GetUrl(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                var path = _bunnyStorage.GetPathFromUrl(fileUrl);
                await _bunnyStorage.DeleteAsync(path);
                _logger.LogInformation("File deleted: {Path}", path);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file");
                return false;
            }
        }

        // =========================
        // Upload Video
        // =========================
        public async Task<FileUploadDto> UploadVideoAsync(Stream fileStream, string fileName, string uploadedBy)
        {
            try
            {
                _logger.LogInformation("Uploading video: {FileName} by user: {UserId}", fileName, uploadedBy);

                // Upload to Bunny
                var path = $"videos/{Guid.NewGuid()}_{fileName}";
                await _bunnyStorage.UploadAsync(path, fileStream);
                var cdnUrl = _bunnyStorage.GetUrl(path);

                // Save metadata
                var fileMetadata = new FileMetadata
                {
                    FileName = fileName,
                    FileType = "Video",
                    FileSize = fileStream.Length,
                    BunnyCdnUrl = cdnUrl,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow
                };
                await _unitOfWork.FileMetadatas.AddAsync(fileMetadata);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Video uploaded successfully: {FileName}", fileName);

                return new FileUploadDto
                {
                    FileName = fileName,
                    FileType = "Video",
                    FileSize = fileStream.Length,
                    Url = cdnUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video: {FileName}", fileName);
                throw;
            }
        }

        // =========================
        // Upload Image
        // =========================
        public async Task<FileUploadDto> UploadImageAsync(Stream fileStream, string fileName, string uploadedBy)
        {
            try
            {
                _logger.LogInformation("Uploading image: {FileName} by user: {UserId}", fileName, uploadedBy);

                // Upload to Bunny
                var path = $"images/{Guid.NewGuid()}_{fileName}";
                await _bunnyStorage.UploadAsync(path, fileStream);
                var cdnUrl = _bunnyStorage.GetUrl(path);

                // Save metadata
                var fileMetadata = new FileMetadata
                {
                    FileName = fileName,
                    FileType = "Image",
                    FileSize = fileStream.Length,
                    BunnyCdnUrl = cdnUrl,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow
                };
                await _unitOfWork.FileMetadatas.AddAsync(fileMetadata);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Image uploaded successfully: {FileName}", fileName);

                return new FileUploadDto
                {
                    FileName = fileName,
                    FileType = "Image",
                    FileSize = fileStream.Length,
                    Url = cdnUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {FileName}", fileName);
                throw;
            }
        }
    }
}
