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

        public string? Remarks { get; set; }

        public List<FypMidEvaluationCriterionItem> Criteria { get; set; } = new();
    }

    public class FypMidEvaluationCriterionItem
    {
        public int CriterionId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal MaxMarks { get; set; }

        [Range(0, 1000)]
        public decimal AwardedMarks { get; set; }

        public string? Comment { get; set; }
    }
}
