namespace KV.Server.Migration.CustomerAvatar;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;

public class Avatar
{
    public Avatar(IBlobContainerFactory blobContainerFactory)
    {
        // _blobContainer = blobContainerFactory.Create(BlobContainers.DEFAULT_PUBLIC); // либо DEFAULT_PUBLIC
    }

    public Avatar()
    {
        // где аватр не null
    }

    public static async Task UploadTicketHistoryAttachmentAsync(string url, Guid Id, IRemoteStreamContent request)
    {
        using (var client = new HttpClient())
        {
            var responce = await client.GetAsync(url);
            var stream = responce.Content.ReadAsStream();
        }

        /*
        await _uploadedFilesRepository.InsertAsync(uploadedFile);
        await this.CurrentUnitOfWork.SaveChangesAsync();

        var ticketHistoryAttachment = new TicketHistoryAttachment()
        {
            TicketHistoryId = ticketHistoryId,
            UploadedFileId = uploadedFile.Id
        };
        await _ticketHistoryAttachmentRepository.InsertAsync(ticketHistoryAttachment);
        var ticketHistory = await _ticketHistoryRepository.GetAsync(ticketHistoryAttachment.TicketHistoryId);
        ticketHistory.TicketHistoryAttachments.Add(ticketHistoryAttachment);
        await _ticketHistoryRepository.UpdateAsync(ticketHistory);
        await _blobContainer.SaveAsync(uploadedFile.Id.ToString(), request.GetStream()); // стрим
        */
    }

    //public async Task<IRemoteStreamContent> GetDownloadAttachmentAsync(Guid fileId)
    //{
    //var file = await _uploadedFilesRepository.FindAsync(fileId);
    //return new RemoteStreamContent(await _blobContainer.GetOrNullAsync(fileId.ToString()), file.Title, file.ContentType);
    //}
}
