using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class FileMetadataRepository : Repository<FileMetadata>, IFileMetadataRepository
    {
        public FileMetadataRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<FileMetadata>> GetFilesByUploaderAsync(string uploaderId)
        {
            try
            {
                return await _dbSet
                    .Where(fm => fm.UploadedBy == uploaderId && !fm.IsDeleted)
                    .OrderByDescending(fm => fm.UploadedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting files for uploader: {UploaderId}", uploaderId);
                throw;
            }
        }

        public async Task<IEnumerable<FileMetadata>> GetFilesByTypeAsync(string fileType)
        {
            try
            {
                return await _dbSet
                    .Where(fm => fm.FileType == fileType && !fm.IsDeleted)
                    .OrderByDescending(fm => fm.UploadedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting files by type: {FileType}", fileType);
                throw;
            }
        }
    }
}
