namespace KV.Server.Tickets;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using KV.Server.Dtos.File;
using KV.Server.Dtos.Tickets;
using KV.Server.File;
using KV.Server.Permissions.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using static System.Net.WebRequestMethods;

public class TicketFileAppService : ApplicationService, ITicketFileAppService
{
    private readonly IBlobContainer _blobContainer;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<TicketAttachment> _ticketAttachmentRepository;
    private readonly IRepository<TicketHistoryAttachment> _ticketHistoryAttachmentRepository;
    private readonly IRepository<TicketHistory, Guid> _ticketHistoryRepository;
    private readonly IRepository<Ticket, long> _ticketsRepository;
    private readonly IRepository<UploadedFile, Guid> _uploadedFilesRepository;
    private readonly ILogger<TicketFileAppService> _logger;
    private readonly IRazorPartialToStringRenderer _razorPartialToStringRenderer;

    public TicketFileAppService(IRepository<Ticket, long> ticketsRepository,
        IRepository<TicketAttachment> ticketAttachmentRepository,
        IRepository<TicketHistory, Guid> ticketHistoryRepository,
        IRepository<TicketHistoryAttachment> ticketHistoryAttachmentRepository,
        IRepository<UploadedFile, Guid> uploadedFilesRepository,
        IBlobContainerFactory blobContainerFactory,
        ILogger<TicketFileAppService> logger,
        IDataFilter dataFilter,
        IRazorPartialToStringRenderer razorPartialToStringRenderer)
    {
        this._ticketsRepository = ticketsRepository;
        this._ticketHistoryRepository = ticketHistoryRepository;
        this._ticketAttachmentRepository = ticketAttachmentRepository;
        this._ticketHistoryAttachmentRepository = ticketHistoryAttachmentRepository;
        this._uploadedFilesRepository = uploadedFilesRepository;
        this._blobContainer = blobContainerFactory.Create(BlobContainers.ATTACHMENTS);
        this._dataFilter = dataFilter;
        this._logger = logger;
        _razorPartialToStringRenderer = razorPartialToStringRenderer;
    }

    [Authorize(TicketPermissions.Tickets.Get)]
    public async Task<List<UploadedFileDto>> GetPrivateTicketAttachmentsListAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            return await this.GetTicketAttachmentsListAsync(ticketId);
        }
    }

    [Authorize(TicketPermissions.Tickets.Get)]
    public async Task<IRemoteStreamContent> GetPrivateDownloadZipByTicketIdAsync(long id)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            return await this.GetDownloadZipByTicketIdAsync(id);
        }
    }

    [Authorize]
    public async Task<List<UploadedFileDto>> GetTicketAttachmentsListAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticketAttachments = (await this._ticketAttachmentRepository
                    .WithDetailsAsync(x => x.UploadedFile))
                .Where(x => x.TicketId == ticketId)
                .ToList();

            var uploadedFiles = new List<UploadedFile>();
            foreach (var ticketAttachment in ticketAttachments)
            {
                uploadedFiles.Add(ticketAttachment.UploadedFile);
            }

            var uploadedFileDtos = this.ObjectMapper.Map<List<UploadedFile>, List<UploadedFileDto>>(uploadedFiles);
            return uploadedFileDtos;
        }
    }

    public async Task<IRemoteStreamContent> GetDownloadZipByTicketIdAsync(long id)
    {
        var ticketAttachments = (await this._ticketAttachmentRepository
        .WithDetailsAsync(x => x.UploadedFile))
                .Where(x => x.TicketId == id)
                .ToList();

        var folder = Path.GetTempPath();
        var zipFileName = $"{GuidGenerator.Create()}.zip";
        var zipFilePath = Path.Combine(folder, zipFileName);
        using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        {
            foreach (var attachment in ticketAttachments)
            {
                var file = attachment.UploadedFile;
                var filePath = Path.Combine(folder, GuidGenerator.Create().ToString());

                using (var fileStream = System.IO.File.Create(filePath))
                {
                    using (var uploadFile = await this._blobContainer.GetOrNullAsync(file.Id.ToString()))
                    {
                        await uploadFile.CopyToAsync(fileStream);
                    }
                }

                archive.CreateEntryFromFile(filePath, file.FileName);
                System.IO.File.Delete(filePath);
            }
        }

        Stream stream = System.IO.File.Open(zipFilePath, FileMode.Open);
        var remoteStream = new RemoteStreamContent(stream,
            $"{id}_{Clock.Now:yyyy_MM_dd_HH_mm}.zip");
        return remoteStream;
    }

    public async Task UploadTicketAttachmentAsync(long ticketId, IRemoteStreamContent request)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var uploadedFile = new UploadedFile
            {
                FileName = request.FileName,
                ContentType = request.ContentType,
                SizeInBytes = request.ContentLength ?? 0,
                Title = request.FileName + " от " + string.Format("{0:dd.MM.yyyy HH:mm}", DateTime.Now)
            };
            await this._uploadedFilesRepository.InsertAsync(uploadedFile);
            await this.CurrentUnitOfWork.SaveChangesAsync();

            var ticketAttachment = new TicketAttachment
            {
                TicketId = ticketId,
                UploadedFileId = uploadedFile.Id
            };
            await this._ticketAttachmentRepository.InsertAsync(ticketAttachment);
            var ticket =
                (await this._ticketsRepository.WithDetailsAsync(x => x.TicketAttachments)).FirstOrDefault(x =>
                    x.Id == ticketAttachment.TicketId);
            ticket.AddAttachment(ticketAttachment);
            await this._ticketsRepository.UpdateAsync(ticket);
            await this._blobContainer.SaveAsync(uploadedFile.Id.ToString(), request.GetStream());
        }
    }
    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Get)]
    public async Task<List<UploadedFileDto>> GetTicketHistoryAttachmentsListAsync(Guid ticketHistoryId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticketHistoryAttachments = (await this._ticketHistoryAttachmentRepository
                    .WithDetailsAsync(x => x.UploadedFile))
                .Where(x => x.TicketHistoryId == ticketHistoryId)
                .ToList();
            var uploadedFiles = new List<UploadedFile>();
            foreach (var ticketHistoryAttachment in ticketHistoryAttachments)
            {
                uploadedFiles.Add(ticketHistoryAttachment.UploadedFile);
            }

            var uploadedFileDtos = this.ObjectMapper.Map<List<UploadedFile>, List<UploadedFileDto>>(uploadedFiles);
            return uploadedFileDtos;
        }
    }
    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Get)]
    public async Task<IRemoteStreamContent> GetDownloadAttachmentAsync(Guid fileId, Guid? tenantId)
    {
        using (this.CurrentTenant.Change(tenantId))
        {
            var uploadedFile = await this._uploadedFilesRepository.FindAsync(fileId);
            var file = await this._blobContainer.GetOrNullAsync(fileId.ToString());
            return new RemoteStreamContent(file, uploadedFile.FileName, uploadedFile.ContentType);
        }
    }
    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Create)]
    public async Task UploadTicketHistoryAttachmentAsync(Guid ticketHistoryId, IRemoteStreamContent request, Guid? tenantId)
    {
        using (this.CurrentTenant.Change(tenantId))
        {
            var uploadedFile = new UploadedFile
            {
                FileName = request.FileName,
                ContentType = request.ContentType,
                SizeInBytes = request.ContentLength ?? 0,
                Title = $"{request.FileName}_{Clock.Now:dd.MM.yyyy}"
            };
            await this._uploadedFilesRepository.InsertAsync(uploadedFile);
            await this.CurrentUnitOfWork.SaveChangesAsync();

            var ticketHistoryAttachment = new TicketHistoryAttachment
            {
                TicketHistoryId = ticketHistoryId,
                UploadedFileId = uploadedFile.Id
            };
            await this._ticketHistoryAttachmentRepository.InsertAsync(ticketHistoryAttachment);
            var ticketHistory = await this._ticketHistoryRepository.GetAsync(ticketHistoryAttachment.TicketHistoryId);
            ticketHistory.TicketHistoryAttachments.Add(ticketHistoryAttachment);
            await this._ticketHistoryRepository.UpdateAsync(ticketHistory);
            await this._blobContainer.SaveAsync(uploadedFile.Id.ToString(), request.GetStream());
        }
    }

    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Get)]
    public async Task<IRemoteStreamContent> GetDownloadZipByTicketHistoryIdAsync(Guid ticketHistoryId, Guid? tenantId)
    {
        using (this.CurrentTenant.Change(tenantId))
        {
            var ticketHistoryAttachments = (await this._ticketHistoryAttachmentRepository
                .WithDetailsAsync(x => x.UploadedFile))
            .Where(x => x.TicketHistoryId == ticketHistoryId)
                .ToList();

            var folder = Path.GetTempPath();
            var zipFileName = $"{GuidGenerator.Create()}.zip";
            var zipFilePath = Path.Combine(folder, zipFileName);

            using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var attachment in ticketHistoryAttachments)
                {
                    var file = attachment.UploadedFile;
                    var tempFileName = $"{GuidGenerator.Create()}";
                    var tempFilePath = Path.Combine(folder, tempFileName);

                    using (var fileStream = System.IO.File.Create(tempFilePath))
                    {
                        using (var uploadFile = await this._blobContainer.GetOrNullAsync(file.Id.ToString()))
                        {
                            await uploadFile.CopyToAsync(fileStream);
                        }
                    }

                    archive.CreateEntryFromFile(tempFilePath, file.FileName);
                    System.IO.File.Delete(tempFilePath);
                }
            }

            var stream = System.IO.File.Open(zipFilePath, FileMode.Open);
            var zipFileNameFormatted = $"attachments_{Clock.Now:yyyy_MM_dd_HH_mm}.zip";
            var remoteStream = new RemoteStreamContent(stream, zipFileNameFormatted);
            return remoteStream;
        }

    }
    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Get)]
    public async Task<IRemoteStreamContent> GetDownloadAllFileInZipByTicketIdAsync(long id, Guid? tenantId)
    {
        using (this.CurrentTenant.Change(tenantId))
        {
            var ticketAttachments = (await this._ticketAttachmentRepository
                .WithDetailsAsync(x => x.UploadedFile))
                .Where(x => x.TicketId == id)
                .Select(p => p.UploadedFile)
                .ToList();

            var ticketHistoryAttachments = (await this._ticketHistoryAttachmentRepository
                .WithDetailsAsync(x => x.UploadedFile))
                .Where(x => x.TicketHistory.TicketId == id)
                .Select(p => p.UploadedFile)
            .ToList();

            var allFiles = ticketAttachments.Union(ticketHistoryAttachments);

            var folder = Path.GetTempPath();
            var zipFileName = $"{GuidGenerator.Create()}.zip";
            var zipFilePath = Path.Combine(folder, zipFileName);
            using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var attachment in allFiles)
                {
                    var file = attachment;
                    var tempFileName = $"{GuidGenerator.Create()}";
                    var tempFilePath = Path.Combine(folder, tempFileName);
                    using (var fileStream = System.IO.File.Create(tempFilePath))
                    {
                        using (var uploadFile = await this._blobContainer.GetOrNullAsync(file.Id.ToString()))
                        {
                            try
                            {
                                await uploadFile.CopyToAsync(fileStream);
                            }
                            catch (Exception exc)
                            {
                                _logger.LogError(exc, $"Не удалось скопировать файл вложения {file.Id}");
                            }
                        }
                    }
                    archive.CreateEntryFromFile(tempFilePath, file.FileName);
                    System.IO.File.Delete(tempFilePath);
                }
            }
            Stream stream = System.IO.File.Open(zipFilePath, FileMode.Open);
            var remoteStream = new RemoteStreamContent(stream, $"{id}_{string.Format("yyyy_MM_dd_HH_mm", Clock.Now)}.zip");
            return remoteStream;
        }

    }
    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Create)]
    public async Task UploadPublicTicketHistoryAttachmentAsync(Guid ticketHistoryId, IRemoteStreamContent request)
    {
        var ticketHistory = await _ticketHistoryRepository.GetAsync(ticketHistoryId);
        using (this.CurrentTenant.Change(ticketHistory.Ticket.TenantId))
        {
            var uploadedFile = new UploadedFile
            {
                FileName = request.FileName,
                ContentType = request.ContentType,
                SizeInBytes = request.ContentLength ?? 0,
                Title = request.FileName + " от " + $"_{Clock.Now:dd.MM.yyyy}.zip"
            };
            await this._uploadedFilesRepository.InsertAsync(uploadedFile);
            await this.CurrentUnitOfWork.SaveChangesAsync();

            var ticketHistoryAttachment = new TicketHistoryAttachment
            {
                TicketHistoryId = ticketHistoryId,
                UploadedFileId = uploadedFile.Id
            };
            await this._ticketHistoryAttachmentRepository.InsertAsync(ticketHistoryAttachment);
            ticketHistory.TicketHistoryAttachments.Add(ticketHistoryAttachment);
            await this._ticketHistoryRepository.UpdateAsync(ticketHistory);
            await this._blobContainer.SaveAsync(uploadedFile.Id.ToString(), request.GetStream());
        }
    }
    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Get)]
    public async Task<IRemoteStreamContent> GetPublicDownloadAttachmentAsync(Guid fileId)
    {
        using (this.CurrentTenant.Change(CurrentTenant.GetId()))
        {
            var uploadedFile = await this._uploadedFilesRepository.FindAsync(fileId);
            var file = await this._blobContainer.GetOrNullAsync(fileId.ToString());
            return new RemoteStreamContent(file, uploadedFile.FileName, uploadedFile.ContentType);
        }
    }
    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Get)]
    public async Task<IRemoteStreamContent> GetPublicDownloadZipByTicketHistoryIdAsync(Guid ticketHistoryId)
    {
        using (this.CurrentTenant.Change(CurrentTenant.GetId()))
        {
            var ticketHistoryAttachments = (await this._ticketHistoryAttachmentRepository
                .WithDetailsAsync(x => x.UploadedFile))
            .Where(x => x.TicketHistoryId == ticketHistoryId)
                .ToList();
            var folder = Path.GetTempPath();
            var zipFileName = $"{GuidGenerator.Create()}.zip";
            var zipFilePath = Path.Combine(folder, zipFileName);
            using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var attachment in ticketHistoryAttachments)
                {
                    var file = attachment.UploadedFile;
                    var tempFileName = $"{GuidGenerator.Create()}";
                    var tempFilePath = Path.Combine(folder, tempFileName);
                    using (var fileStream = System.IO.File.Create(tempFilePath))
                    {
                        using (var uploadFile = await this._blobContainer.GetOrNullAsync(file.Id.ToString()))
                        {
                            await uploadFile.CopyToAsync(fileStream);
                        }
                    }
                    archive.CreateEntryFromFile(tempFilePath, file.FileName);
                    System.IO.File.Delete(tempFilePath);
                }
            }
            Stream stream = System.IO.File.Open(zipFilePath, FileMode.Open);
            var remoteStream = new RemoteStreamContent(stream, $"attachments_{Clock.Now:yyyy_MM_dd_HH_mm}.zip");
            return remoteStream;
        }

    }
    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Get)]
    public async Task<IRemoteStreamContent> GetPublicDownloadAllFileInZipByTicketIdAsync(long id)
    {
        using (this.CurrentTenant.Change(CurrentTenant.GetId()))
        {
            var ticketAttachments = (await this._ticketAttachmentRepository
                .WithDetailsAsync(x => x.UploadedFile))
                .Where(x => x.TicketId == id)
            .Select(p => p.UploadedFile)
                .ToList();

            var ticketHistoryAttachments = (await this._ticketHistoryAttachmentRepository
                .WithDetailsAsync(x => x.UploadedFile))
                .Where(x => x.TicketHistory.TicketId == id)
                .Select(p => p.UploadedFile)
            .ToList();

            var allFiles = ticketAttachments.Union(ticketHistoryAttachments);

            var folder = Path.GetTempPath();
            var zipFileName = $"{GuidGenerator.Create()}.zip";
            var zipFilePath = Path.Combine(folder, zipFileName);
            using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var attachment in allFiles)
                {
                    var file = attachment;
                    var filePath = Path.Combine(folder, GuidGenerator.Create().ToString());
                    using (var fileStream = System.IO.File.Create(filePath))
                    {
                        using (var uploadFile = await this._blobContainer.GetOrNullAsync(file.Id.ToString()))
                        {
                            try
                            {
                                await uploadFile.CopyToAsync(fileStream);
                            }
                            catch { }
                        }
                    }
                    archive.CreateEntryFromFile(filePath, file.FileName);
                    System.IO.File.Delete(filePath);
                }
            }
            Stream stream = System.IO.File.Open(zipFilePath, FileMode.Open);
            var remoteStream = new RemoteStreamContent(stream, $"{id}_{Clock.Now:yyyy_MM_dd_HH_mm}.zip");
            return remoteStream;
        }

    }
    [Authorize(TicketAttachmentsPermissions.TicketAttachments.Get)]
    public async Task<IRemoteStreamContent> GetPdfFileStreamAsync(long ticketId, Guid fileId)
    {
        using (this.CurrentTenant.Change(CurrentTenant.GetId()))
        {
            var ticket = (await _ticketsRepository.WithDetailsAsync(x => x.TicketHistory)).FirstOrDefault(x => x.Id == ticketId);
            var ticketDto = ObjectMapper.Map<Ticket, TicketDetailsDto>(ticket);

            var razorString = await _razorPartialToStringRenderer.RenderPartialToStringAsync("DetailsPreview.cshtml", ticketDto);
            var bytes = await ConvertRazorStringToPdf(razorString);
            var uploadedFile = await this._uploadedFilesRepository.FindAsync(fileId);
            return new ByteStreamContent(bytes, uploadedFile.FileName, uploadedFile.ContentType);
        }
    }
    private async Task<byte[]> ConvertRazorStringToPdf(string razorString)
    {
        byte[] pdfBytes;

        using (MemoryStream ms = new MemoryStream())
        {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            // Преобразование строки Razor в PDF
            StringReader sr = new StringReader(razorString);
            XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, sr);

            document.Close();
            pdfBytes = ms.ToArray();
        }

        return pdfBytes;
    }
}
public class ByteStreamContent : IRemoteStreamContent
{
    private readonly byte[] _bytes;

    public ByteStreamContent(byte[] bytes, string fileName, string contentType)
    {
        _bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        FileName = fileName;
        ContentType = contentType;
    }

    public string FileName { get; }

    public string ContentType { get; }

    public long? ContentLength => _bytes.Length;


    public async Task CopyToAsync(Stream stream, CancellationToken cancellationToken)
    {
        using (var byteStream = new MemoryStream(_bytes))
        {
            await byteStream.CopyToAsync(stream, cancellationToken);
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Stream GetStream()
    {
        return new MemoryStream(_bytes);
    }

}

