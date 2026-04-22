using EMAP.Domain.Fyp;

namespace EMAP.Web.Services.Fyp
{
    public interface IFypFinalRecordService
    {
        Task<(bool Success, string Message, int? FinalRecordId)> CreateFinalRecordAsync(
            int groupId,
            string submittedByUserId,
            string? coordinatorRemarks);

        Task<(bool Success, string Message)> MarkProcessedAsync(
            int finalRecordId,
            string processedByUserId,
            string? adminRemarks);

        Task<(bool Success, string Message)> ArchiveAsync(
            int finalRecordId,
            string archivedByUserId);
    }
}
