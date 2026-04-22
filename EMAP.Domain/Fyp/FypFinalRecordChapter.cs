using System;

namespace EMAP.Domain.Fyp
{
    public class FypFinalRecordChapter
    {
        public int Id { get; set; }

        public int FinalRecordId { get; set; }
        public FypFinalRecord FinalRecord { get; set; } = null!;

        public FypStage Stage { get; set; }

        public FypChapterType ChapterType { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime? SubmittedAt { get; set; }

        public ChapterSubmissionStatus Status { get; set; }

        public string? Feedback { get; set; }
    }
}
