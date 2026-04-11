using System;
using System.ComponentModel.DataAnnotations;

namespace EMAP.Domain.Fyp
{
    public enum FypChapterType
    {
        // FYP-1
        VisionAndScope = 1,
        Srs = 2,
        SystemOverview = 3,

        // FYP-2
        SystemImplementation = 4,
        SystemTestingAndDevelopment = 5,
        ResultsAndDiscussion = 6
    }

    public class FypChapterAnnouncement
    {
        public int Id { get; set; }

        [Required]
        public int FypCallId { get; set; }
        public FypCall? FypCall { get; set; }

        [Required]
        public FypChapterType ChapterType { get; set; }

        [Required]
        public FypStage Stage { get; set; } = FypStage.Fyp1;

        public bool IsOpen { get; set; } = false;

        public DateTime? Deadline { get; set; }

        [StringLength(1000)]
        public string? Instructions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
