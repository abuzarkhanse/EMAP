using System;
using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class SubmitChapterViewModel
    {
        // Required for submission
        public int ChapterAnnouncementId { get; set; }

        // Display information
        public string Title { get; set; } = string.Empty;
        public FypChapterType ChapterType { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Instructions { get; set; }

        public List<FypChapterSubmission> MyChapters { get; set; } = new();
    }
}
