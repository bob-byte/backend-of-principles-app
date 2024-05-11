using AutoMapper;

using SET.DataAccess;

using Microsoft.AspNetCore.Http;

using SET.Shared.Models;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic;

public class FileSystemService : IFileSystemService
{
    private readonly AppDbContext m_context;
    private readonly IMapper m_mapper;

    public FileSystemService( AppDbContext context, IMapper mapper )
    {
        m_context = context;
        m_mapper = mapper;
    }

    public MemoryStream GetFilesById( long userId )
    {
        IQueryable<FileEntity> fileEntities = m_context.FileEntities.Where( x => x.UserId == userId );

        var memoryStream = new MemoryStream();

        using (ZipArchive zipArchive = new( memoryStream, ZipArchiveMode.Create, leaveOpen: true ))
        {
            foreach (FileEntity fileEntity in fileEntities)
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry( fileEntity.Id.ToString() + fileEntity.FileExtension );

                string filePath = Path.Combine(
                    "../DAL/Files/",
                    fileEntity.Id.ToString() + fileEntity.FileExtension
                );
                using (var fileStream = new FileStream( filePath, FileMode.Open ))
                using (Stream entryStream = entry.Open())
                {
                    fileStream.CopyTo( entryStream );
                }
            }
        }
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task SaveFileAsync( FileEntityDto fileEntityDTO, long userId, IFormFile file )
    {
        if (file != null && file.Length > 0)
        {
            FileEntity fileEntity = m_mapper.Map<FileEntity>( fileEntityDTO );

            //fileEntity.SecondId = Guid.NewGuid();
            fileEntity.UserId = userId;
            fileEntity.RecordDate = DateTime.Now;
            fileEntity.FileExtension = Path.GetExtension( file.FileName );

            string filePath = Path.Combine( "../DAL/Files/", fileEntity.Id.ToString() + fileEntity.FileExtension );

            using (FileStream stream = File.Create( filePath ))
            {
                await file.CopyToAsync( stream );
            }

            await m_context.FileEntities.AddAsync( fileEntity );
            await m_context.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException( message: "No file was send or file has zero length.", paramName: nameof(file) );
        }
    }
}
