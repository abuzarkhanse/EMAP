using System.Collections.Generic;

namespace EMAP.Web.ViewModels.Fyp
{
    public class StudentPublishedEvaluationCardViewModel
    {
        public int EvaluationId { get; set; }

        public string EvaluationTitle { get; set; } = string.Empty;

        public string ProjectTitle { get; set; } = string.Empty;

        public string Batch { get; set; } = string.Empty;

        public string ProgramCode { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public string? RegistrationNo { get; set; }

        public string SupervisorName { get; set; } = string.Empty;

        public string? Venue { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public decimal TotalMarks { get; set; }

        public decimal WeightedMarks { get; set; }

        public decimal WeightagePercent { get; set; }

        public string? OverallRemarks { get; set; }

        public string? StudentRemarks { get; set; }

        public string? EvaluatorName { get; set; }

        public bool ShowCommitteeToStudent { get; set; }

        public List<StudentPublishedEvaluationCriterionViewModel> Criteria { get; set; } = new();
    }

    public class StudentPublishedEvaluationCriterionViewModel
    {
        public string CriterionTitle { get; set; } = string.Empty;

        public decimal MaxMarks { get; set; }

        public decimal AwardedMarks { get; set; }

        public string? Comment { get; set; }
    }
}
