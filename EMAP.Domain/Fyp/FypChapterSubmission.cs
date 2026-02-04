using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAP.Domain.Fyp
{
    public class FypChapterSubmission
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public int ChapterAnnouncementId { get; set; }
        public string Title { get; set; } = "";
        public string FilePath { get; set; } = "";
        public DateTime SubmittedAt { get; set; }

        public ChapterSubmissionStatus Status { get; set; } = ChapterSubmissionStatus.Submitted;
        public string? Feedback { get; set; }

        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedById { get; set; }

        public StudentGroup Group { get; set; } = null!;
        public FypChapterAnnouncement ChapterAnnouncement { get; set; } = null!;
        public string SupervisorId { get; set; } = null!;

    }
}
