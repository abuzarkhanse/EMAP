using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMAP.Domain.Fyp
{
    public enum FypFinalRecordStatus
    {
        PendingAdminReview = 0,
        Processed = 1,
        Archived = 2
    }

    public class FypFinalRecord
    {
        public int Id { get; set; }

        public int StudentGroupId { get; set; }
        public StudentGroup StudentGroup { get; set; } = null!;

        public int FypCallId { get; set; }

        public int DepartmentId { get; set; }

        [StringLength(300)]
        public string ProjectTitle { get; set; } = string.Empty;

        [StringLength(50)]
        public string ProgramCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string Batch { get; set; } = string.Empty;

        [StringLength(200)]
        public string SupervisorName { get; set; } = string.Empty;

        public string? FypDescription { get; set; }

        public bool IsFypCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? CompletionRemarks { get; set; }

        public decimal? Fyp1AverageMarks { get; set; }
        public decimal? Fyp2AverageMarks { get; set; }
        public decimal? FinalAverageMarks { get; set; }

        public string SubmittedByUserId { get; set; } = string.Empty;
        public DateTime SubmittedToAdminAt { get; set; }

        public string? CoordinatorRemarks { get; set; }

        public string? ProcessedByUserId { get; set; }
        public DateTime? ProcessedByAdminAt { get; set; }

        public string? AdminRemarks { get; set; }

        public FypFinalRecordStatus Status { get; set; } = FypFinalRecordStatus.PendingAdminReview;

        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }

        public ICollection<FypFinalRecordStudent> Students { get; set; } = new List<FypFinalRecordStudent>();
        public ICollection<FypFinalRecordChapter> Chapters { get; set; } = new List<FypFinalRecordChapter>();
        public ICollection<FypFinalRecordEvaluation> Evaluations { get; set; } = new List<FypFinalRecordEvaluation>();
    }
}
