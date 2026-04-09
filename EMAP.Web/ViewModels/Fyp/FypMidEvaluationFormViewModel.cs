using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMAP.Web.ViewModels.Fyp
{
    public class FypMidEvaluationFormViewModel
    {
        public int EvaluationId { get; set; }

        public int GroupId { get; set; }

        public string ProjectTitle { get; set; } = string.Empty;

        public string Batch { get; set; } = string.Empty;

        public string ProgramCode { get; set; } = string.Empty;

        public string SupervisorName { get; set; } = string.Empty;

        public DateTime? ScheduledAt { get; set; }

        public string? Venue { get; set; }

        public decimal WeightagePercent { get; set; }

        public string? OverallRemarks { get; set; }

        [Required]
        public string EvaluatorName { get; set; } = string.Empty;

        public List<MemberEvaluationRowViewModel> Members { get; set; } = new();

        public List<CriterionHeaderViewModel> Criteria { get; set; } = new();
    }

    public class MemberEvaluationRowViewModel
    {
        public int EvaluationMemberId { get; set; }

        public string StudentUserId { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public string? RegistrationNo { get; set; }

        public string? Remarks { get; set; }

        public decimal TotalMarks { get; set; }

        public decimal WeightedMarks { get; set; }

        public List<MemberCriterionScoreViewModel> Scores { get; set; } = new();
    }

    public class MemberCriterionScoreViewModel
    {
        public int CriterionId { get; set; }

        public string CriterionTitle { get; set; } = string.Empty;

        public decimal MaxMarks { get; set; }

        public decimal AwardedMarks { get; set; }

        public string? Comment { get; set; }
    }

    public class CriterionHeaderViewModel
    {
        public int CriterionId { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal MaxMarks { get; set; }
    }
}
