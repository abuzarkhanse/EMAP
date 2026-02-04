using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EMAP.Domain.Fyp
{
    public enum FypChapterType
    {
        VisionAndScope = 1,
        Srs = 2,
        SystemOverview = 3
    }

    public class FypChapterAnnouncement
    {
        public int Id { get; set; }

        [Required]
        public int FypCallId { get; set; }
        public FypCall? FypCall { get; set; }

        [Required]
        public FypChapterType ChapterType { get; set; }

        public bool IsOpen { get; set; } = false;

        public DateTime? Deadline { get; set; }

        [StringLength(1000)]
        public string? Instructions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
