using BusinessLogic;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SET.Shared.Models;
using System;
using System.Threading.Tasks;

namespace SET.WebAPI.Controllers;

//[Route("api/[controller]")]
//[ApiController]
//public class FileSystemController : ControllerBase
//{
//    private readonly IFileSystemService _fileSystemService;

//    public FileSystemController(IFileSystemService fileSystemService)
//    {
//        _fileSystemService = fileSystemService;
//    }

//    [HttpPost("{userId}/{fileType}/{filePurpose}")]
//    public async Task<IActionResult> SetFile([FromForm] IFormFile file, Guid userId, FileType fileType, FilePurpose filePurpose)
//    {
//        var fileEntityDTO = new FileEntityDto
//        {
//            FileType = fileType,
//            FilePurpose = filePurpose
//        };
//        try
//        {
//            await _fileSystemService.SaveFileAsync(fileEntityDTO, userId, file);
//            return Ok();
//        }
//        catch (Exception e)
//        {
//            return BadRequest(e.Message);
//        }
//    }

//    [HttpGet("{userId}")]
//    public IActionResult GetFiles(Guid userId)
//    {
//        return File(_fileSystemService.GetFilesById(userId), "application/zip", "files.zip");
//    }
//}
