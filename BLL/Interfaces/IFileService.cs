using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadDto> UploadVideoAsync(Stream fileStream, string fileName, string uploadedBy);
        Task<FileUploadDto> UploadImageAsync(Stream fileStream, string fileName, string uploadedBy);
        Task<FileUploadDto> UploadPdfAsync(Stream fileStream, string fileName, string uploadedBy);
        Task<bool> DeleteFileAsync(string url, string userId);
        Task<List<FileUploadDto>> GetUserFilesAsync(string userId);
    }
}
