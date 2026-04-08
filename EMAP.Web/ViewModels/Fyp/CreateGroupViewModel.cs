using System.ComponentModel.DataAnnotations;

namespace EMAP.Web.ViewModels.Fyp
{
    public class CreateGroupViewModel
    {
        public string Batch { get; set; } = string.Empty;
        public string ActiveCallTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expected project title is required.")]
        [Display(Name = "Expected / Tentative Project Title")]
        [StringLength(300)]
        public string TentativeProjectTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Program code is required.")]
        [Display(Name = "Program Code")]
        [StringLength(50)]
        public string ProgramCode { get; set; } = string.Empty;

        [EmailAddress]
        public string? Member2Email { get; set; }

        [EmailAddress]
        public string? Member3Email { get; set; }
    }
}
