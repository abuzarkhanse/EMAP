using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class EvaluationBulkCreateViewModel
    {
        [Display(Name = "Evaluation Type / Milestone")]
        public int MilestoneId { get; set; }

        [Display(Name = "Weightage (%)")]
        [Range(0, 100)]
        public decimal WeightagePercent { get; set; } = 20;

        [Display(Name = "Scheduled Date & Time")]
        public DateTime? ScheduledAt { get; set; }

        public string? Venue { get; set; }
        public string? Instructions { get; set; }
        public string? CommitteeMembers { get; set; }

        public List<int> SelectedGroupIds { get; set; } = new();

        public List<EvaluationEligibleGroupItemViewModel> EligibleGroups { get; set; } = new();
    }

    public class EvaluationEligibleGroupItemViewModel
    {
        public int GroupId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string ProgramCode { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = "-";
        public FypStage Stage { get; set; }
        public string ReadinessText { get; set; } = string.Empty;
    }
}
