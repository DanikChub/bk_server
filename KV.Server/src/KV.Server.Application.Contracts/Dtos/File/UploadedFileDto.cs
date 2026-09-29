namespace KV.Server.Dtos.File;
using System;
using Volo.Abp.Application.Dtos;

public class UploadedFileDto : AuditedEntityDto<Guid>
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string Title { get; set; }
    public long SizeInBytes { get; set; }
    public string Url { get; set; }
}
