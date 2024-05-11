using System;

namespace SET.Shared.Models;

public class FileEntity
{
    public long Id { get; set; }
    public string FileExtension { get; set; }
    public FileType FileType { get; set; }
    public FilePurpose FilePurpose { get; set; }
    public long? UserId { get; set; }
    public User User { get; set; }
    public DateTime RecordDate { get; set; }
}
