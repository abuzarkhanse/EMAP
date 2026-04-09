using System.ComponentModel.DataAnnotations;

namespace EMAP.Web.ViewModels.Fyp
{
    public class FypMidEvaluationFormViewModel
    {
        public int EvaluationId { get; set; }

        public int StudentGroupId { get; set; }

        public string GroupTitle { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public string ProgramCode { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime? ScheduledAt { get; set; }

        [Range(0, 100)]
        public decimal WeightagePercent { get; set; } = 20;

        public decimal WeightedMarks { get; set; }

        public string? EvaluatorName { get; set; }

        public string? Remarks { get; set; }

        public List<FypMidEvaluationCriterionItem> Criteria { get; set; } = new();
    }

    public class FypMidEvaluationCriterionItem
    {
        public int CriterionId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal MaxMarks { get; set; }

        [Range(0, 5)]
        public decimal AwardedMarks { get; set; }

        public string? Comment { get; set; }
    }
}
