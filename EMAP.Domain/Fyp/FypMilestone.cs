using System;
using System.Collections.Generic;

namespace EMAP.Domain.Fyp
{
    public class FypMilestone
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public FypStage Stage { get; set; }

        public FypMilestoneType Type { get; set; }

        public int? ChapterNumber { get; set; }

        public bool IsOptional { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime? DueDate { get; set; }

        public int DisplayOrder { get; set; }

        public ICollection<FypEvaluation> Evaluations { get; set; } = new List<FypEvaluation>();
        public ICollection<FypChapterSubmission> ChapterSubmissions { get; set; } = new List<FypChapterSubmission>();
    }
}