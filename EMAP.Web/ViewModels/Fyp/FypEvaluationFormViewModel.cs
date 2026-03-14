using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMAP.Web.ViewModels.Fyp
{
    public class FypEvaluationFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Student Group")]
        public int StudentGroupId { get; set; }

        [Required]
        [Display(Name = "Milestone")]
        public int MilestoneId { get; set; }

        [Display(Name = "Scheduled Date & Time")]
        public DateTime? ScheduledAt { get; set; }

        [StringLength(300)]
        public string? Venue { get; set; }

        [StringLength(2000)]
        public string? Instructions { get; set; }

        [Display(Name = "Committee Members")]
        [StringLength(2000)]
        public string? CommitteeMembers { get; set; }

        [Display(Name = "Publish to Student")]
        public bool IsPublishedToStudent { get; set; }

        [Display(Name = "Show Committee to Student")]
        public bool ShowCommitteeToStudent { get; set; }

        public List<SelectListItem> GroupOptions { get; set; } = new();
        public List<SelectListItem> MilestoneOptions { get; set; } = new();
    }
}