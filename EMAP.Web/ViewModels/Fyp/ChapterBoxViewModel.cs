using System;
using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class ChapterBoxViewModel
    {
        public int ChapterAnnouncementId { get; set; }
        public FypChapterType ChapterType { get; set; }

        public bool IsOpen { get; set; }
        public DateTime? Deadline { get; set; }

        // Submission info (latest submission for this chapter)
        public int? SubmissionId { get; set; }
        public ChapterSubmissionStatus? Status { get; set; }
        public string? Feedback { get; set; }
        public DateTime? SubmittedAt { get; set; }


        // UI helpers
        public bool IsUnlocked { get; set; }          // based on previous chapter completion
        public bool CanSubmitNow { get; set; }        // IsOpen && IsUnlocked && leader-only (optional)
        public bool CanResubmit { get; set; }         // ChangesRequested
        public bool IsCompleted => Status == ChapterSubmissionStatus.CoordinatorApproved;
    }
}