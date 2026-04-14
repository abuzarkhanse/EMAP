using System.ComponentModel.DataAnnotations;
using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class FypEvaluationCriterionFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Evaluation Type")]
        public FypMilestoneType EvaluationType { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Rubric Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.5, 1000)]
        [Display(Name = "Maximum Marks")]
        public decimal MaxMarks { get; set; }

        [Range(1, 999)]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
