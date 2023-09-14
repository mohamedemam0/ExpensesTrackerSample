namespace ExpensesTracker.Server.Data.Interfaces
{
    public interface IAttachmentRepo
    {
        Task AddAsync(Attachment attachment);
        Task<IEnumerable<Attachment>> GetUnsedAttachmentsAsync(int hours);
        Task DeleteAsync(string id, string uploadedByUserId);
        Task<IEnumerable<Attachment>> GetByURLAsync(string[] urls);
        Task DeleteBatchAsync(IEnumerable<Attachment> attachments);
    }
}