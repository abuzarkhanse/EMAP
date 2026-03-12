using System;
using System.Collections.Generic;

namespace EMAP.Domain.Fyp
{
    public class FypEvaluation
    {
        public int Id { get; set; }

        public int StudentGroupId { get; set; }
        public StudentGroup StudentGroup { get; set; } = default!;

        public int MilestoneId { get; set; }
        public FypMilestone Milestone { get; set; } = default!;

        public string EvaluatorUserId { get; set; } = string.Empty;

        public DateTime? ScheduledAt { get; set; }

        public decimal TotalMarks { get; set; }

        public string? Remarks { get; set; }

        public FypEvaluationStatus Status { get; set; } = FypEvaluationStatus.Draft;

        public bool IsSubmitted { get; set; } = false;

        public DateTime? SubmittedAt { get; set; }

        public ICollection<FypEvaluationScore> Scores { get; set; } = new List<FypEvaluationScore>();

        public string? Venue { get; set; }

        public string? Instructions { get; set; }

        public string? CommitteeMembers { get; set; }

        public bool IsPublishedToStudent { get; set; } = false;

        public bool ShowCommitteeToStudent { get; set; } = false;
    }
}