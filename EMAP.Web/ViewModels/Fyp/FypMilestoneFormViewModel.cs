using System;
using System.ComponentModel.DataAnnotations;
using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class FypMilestoneFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Stage")]
        public FypStage Stage { get; set; }

        [Required]
        [Display(Name = "Milestone Type")]
        public FypMilestoneType Type { get; set; }

        [Display(Name = "Chapter Number")]
        public int? ChapterNumber { get; set; }

        [Display(Name = "Optional")]
        public bool IsOptional { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Required]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }
    }
}