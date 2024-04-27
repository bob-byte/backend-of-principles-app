using Microsoft.AspNetCore.Http;

using SET.Shared.Models;

using System;
using System.IO;
using System.Threading.Tasks;

namespace BusinessLogic;

public interface IFileSystemService
{
    MemoryStream GetFilesById( Guid userId );
    Task SaveFileAsync( FileEntityDto fileEntityDTO, Guid userId, IFormFile file );
}