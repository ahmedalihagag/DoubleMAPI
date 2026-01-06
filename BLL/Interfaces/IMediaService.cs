using BLL.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IMediaService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<FileUploadDto> UploadVideoAsync(Stream fileStream, string fileName, string uploadedBy);
        Task<FileUploadDto> UploadImageAsync(Stream fileStream, string fileName, string uploadedBy);
    }
}
