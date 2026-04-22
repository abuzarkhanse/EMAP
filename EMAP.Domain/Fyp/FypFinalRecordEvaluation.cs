using System;

namespace EMAP.Domain.Fyp
{
    public class FypFinalRecordEvaluation
    {
        public int Id { get; set; }

        public int FinalRecordId { get; set; }
        public FypFinalRecord FinalRecord { get; set; } = null!;

        public FypStage Stage { get; set; }

        public FypMilestoneType EvaluationType { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime? ScheduledAt { get; set; }

        public string? Venue { get; set; }

        public string? EvaluatorName { get; set; }

        public decimal TotalMarks { get; set; }

        public decimal WeightagePercent { get; set; }

        public decimal WeightedMarks { get; set; }

        public string? Remarks { get; set; }

        public bool IsPublishedToStudent { get; set; }

        public ICollection<FypFinalRecordEvaluationMember> Members { get; set; } = new List<FypFinalRecordEvaluationMember>();
    }
}
