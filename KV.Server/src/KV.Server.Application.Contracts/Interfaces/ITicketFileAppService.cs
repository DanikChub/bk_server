namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Dtos.File;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

public interface ITicketFileAppService : IApplicationService
{
    Task<List<UploadedFileDto>> GetPrivateTicketAttachmentsListAsync(long ticketId);
    Task<List<UploadedFileDto>> GetTicketAttachmentsListAsync(long ticketId);
    Task UploadTicketAttachmentAsync(long ticketId, IRemoteStreamContent request);
    Task<IRemoteStreamContent> GetPrivateDownloadZipByTicketIdAsync(long id);
    Task<IRemoteStreamContent> GetDownloadZipByTicketIdAsync(long id);
    Task<List<UploadedFileDto>> GetTicketHistoryAttachmentsListAsync(Guid ticketHistoryId);
    Task UploadTicketHistoryAttachmentAsync(Guid ticketHistoryId, IRemoteStreamContent request, Guid? tenantId);

    Task<IRemoteStreamContent> GetDownloadAttachmentAsync(Guid fileId, Guid? tenantId);

    Task<IRemoteStreamContent> GetDownloadZipByTicketHistoryIdAsync(Guid ticketHistoryId, Guid? tenantId);
    Task<IRemoteStreamContent> GetDownloadAllFileInZipByTicketIdAsync(long id, Guid? tenantId);


    Task<IRemoteStreamContent> GetPublicDownloadZipByTicketHistoryIdAsync(Guid ticketHistoryId);
    Task<IRemoteStreamContent> GetPublicDownloadAllFileInZipByTicketIdAsync(long id);
    Task<IRemoteStreamContent> GetPublicDownloadAttachmentAsync(Guid fileId);
    Task UploadPublicTicketHistoryAttachmentAsync(Guid ticketHistoryId, IRemoteStreamContent request);
    Task<IRemoteStreamContent> GetPdfFileStreamAsync(long ticketId, Guid fileId);
}
