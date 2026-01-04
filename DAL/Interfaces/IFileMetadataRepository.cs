using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IFileMetadataRepository : IRepository<FileMetadata>
    {
        Task<IEnumerable<FileMetadata>> GetFilesByUploaderAsync(string uploaderId);
        Task<IEnumerable<FileMetadata>> GetFilesByTypeAsync(string fileType);
    }
}
