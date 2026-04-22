using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using EMAP.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Web.Services.Fyp
{
    public class FypFinalRecordService : IFypFinalRecordService
    {
        private readonly EmapDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FypFinalRecordService(
            EmapDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<(bool Success, string Message, int? FinalRecordId)> CreateFinalRecordAsync(
            int groupId,
            string submittedByUserId,
            string? coordinatorRemarks)
        {
            var group = await _db.StudentGroups
                .Include(g => g.FypCall)
                .Include(g => g.Supervisor)
                .Include(g => g.Evaluations)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return (false, "Student group not found.", null);

            if (!group.IsFypCompleted)
                return (false, "Only fully completed FYP groups can be sent to admin.", null);

            var alreadyExists = await _db.FypFinalRecords
                .AnyAsync(x => x.StudentGroupId == groupId);

            if (alreadyExists)
                return (false, "This group has already been sent to admin.", null);

            var chapterSubmissions = await _db.FypChapterSubmissions
                .Include(x => x.ChapterAnnouncement)
                .Where(x => x.GroupId == groupId)
                .OrderBy(x => x.Stage)
                .ThenBy(x => x.ChapterAnnouncement.ChapterType)
                .ToListAsync();

            var evaluations = await _db.FypEvaluations
                .Include(e => e.Milestone)
                .Include(e => e.Members)
                .Where(e => e.StudentGroupId == groupId)
                .OrderBy(e => e.Milestone.Stage)
                .ThenBy(e => e.Milestone.Type)
                .ToListAsync();

            var studentIds = new List<(string UserId, string Role)>
            {
                (group.LeaderId, "Leader")
            };

            if (!string.IsNullOrWhiteSpace(group.Member2Id))
                studentIds.Add((group.Member2Id!, "Member 2"));

            if (!string.IsNullOrWhiteSpace(group.Member3Id))
                studentIds.Add((group.Member3Id!, "Member 3"));

            var finalRecord = new FypFinalRecord
            {
                StudentGroupId = group.Id,
                FypCallId = group.FypCallId,
                DepartmentId = group.DepartmentId,
                ProjectTitle = group.TentativeProjectTitle,
                ProgramCode = group.ProgramCode,
                Batch = group.FypCall?.Batch ?? string.Empty,
                SupervisorName = group.Supervisor?.Name ?? "-",
                FypDescription = group.FypDescription,
                IsFypCompleted = group.IsFypCompleted,
                CompletedAt = group.CompletedAt,
                CompletionRemarks = group.CompletionRemarks,
                SubmittedByUserId = submittedByUserId,
                SubmittedToAdminAt = DateTime.UtcNow,
                CoordinatorRemarks = coordinatorRemarks,
                Status = FypFinalRecordStatus.PendingAdminReview,
                Fyp1AverageMarks = evaluations
                    .Where(x => x.Milestone.Stage == FypStage.Fyp1)
                    .Select(x => (decimal?)x.WeightedMarks)
                    .DefaultIfEmpty(0)
                    .Average(),
                Fyp2AverageMarks = evaluations
                    .Where(x => x.Milestone.Stage == FypStage.Fyp2)
                    .Select(x => (decimal?)x.WeightedMarks)
                    .DefaultIfEmpty(0)
                    .Average(),
                FinalAverageMarks = evaluations
                    .Select(x => (decimal?)x.WeightedMarks)
                    .DefaultIfEmpty(0)
                    .Average()
            };

            foreach (var item in studentIds)
            {
                var user = await _userManager.FindByIdAsync(item.UserId);
                if (user == null) continue;

                finalRecord.Students.Add(new FypFinalRecordStudent
                {
                    StudentUserId = user.Id,
                    StudentName = !string.IsNullOrWhiteSpace(user.FullName)
                        ? user.FullName
                        : (user.UserName ?? user.Email ?? "Student"),
                    RegistrationNo = user.RegistrationNumber ?? user.UserName ?? "-",
                    Email = user.Email ?? "-",
                    RoleInGroup = item.Role
                });
            }

            foreach (var chapter in chapterSubmissions)
            {
                finalRecord.Chapters.Add(new FypFinalRecordChapter
                {
                    Stage = chapter.Stage,
                    ChapterType = chapter.ChapterAnnouncement.ChapterType,
                    Title = chapter.Title,
                    SubmittedAt = chapter.SubmittedAt,
                    Status = chapter.Status,
                    Feedback = chapter.Feedback
                });
            }

            foreach (var eval in evaluations)
            {
                var finalEval = new FypFinalRecordEvaluation
                {
                    Stage = eval.Milestone.Stage,
                    EvaluationType = eval.Milestone.Type,
                    Title = eval.Milestone.Title,
                    ScheduledAt = eval.ScheduledAt,
                    Venue = eval.Venue,
                    EvaluatorName = eval.EvaluatorName,
                    TotalMarks = eval.TotalMarks,
                    WeightagePercent = eval.WeightagePercent,
                    WeightedMarks = eval.WeightedMarks,
                    Remarks = eval.Remarks,
                    IsPublishedToStudent = eval.IsPublishedToStudent
                };

                foreach (var member in eval.Members.OrderBy(x => x.StudentName))
                {
                    finalEval.Members.Add(new FypFinalRecordEvaluationMember
                    {
                        StudentUserId = member.StudentUserId,
                        StudentName = member.StudentName,
                        RegistrationNo = member.RegistrationNo ?? "-",
                        TotalMarks = member.TotalMarks,
                        WeightedMarks = member.WeightedMarks,
                        Remarks = member.Remarks
                    });
                }

                finalRecord.Evaluations.Add(finalEval);
            }

            _db.FypFinalRecords.Add(finalRecord);
            await _db.SaveChangesAsync();

            return (true, "Final academic record successfully sent to admin.", finalRecord.Id);
        }

        public async Task<(bool Success, string Message)> MarkProcessedAsync(
            int finalRecordId,
            string processedByUserId,
            string? adminRemarks)
        {
            var record = await _db.FypFinalRecords.FirstOrDefaultAsync(x => x.Id == finalRecordId);
            if (record == null)
                return (false, "Final record not found.");

            if (record.IsArchived)
                return (false, "Archived record cannot be processed.");

            record.Status = FypFinalRecordStatus.Processed;
            record.ProcessedByUserId = processedByUserId;
            record.ProcessedByAdminAt = DateTime.UtcNow;
            record.AdminRemarks = adminRemarks;

            await _db.SaveChangesAsync();
            return (true, "Final record marked as processed.");
        }

        public async Task<(bool Success, string Message)> ArchiveAsync(
            int finalRecordId,
            string archivedByUserId)
        {
            var record = await _db.FypFinalRecords.FirstOrDefaultAsync(x => x.Id == finalRecordId);
            if (record == null)
                return (false, "Final record not found.");

            record.IsArchived = true;
            record.ArchivedAt = DateTime.UtcNow;
            record.Status = FypFinalRecordStatus.Archived;

            if (string.IsNullOrWhiteSpace(record.ProcessedByUserId))
                record.ProcessedByUserId = archivedByUserId;

            if (!record.ProcessedByAdminAt.HasValue)
                record.ProcessedByAdminAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return (true, "Final record archived successfully.");
        }
    }
}
