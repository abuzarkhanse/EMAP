using System.ComponentModel.DataAnnotations;

namespace EMAP.Web.ViewModels.Fyp
{
    public class FypCommitteeFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Session { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CoordinatorEmail { get; set; } = string.Empty;

        [EmailAddress]
        public string? ConvenorEmail { get; set; }

        [Required]
        public string ProgramCodesCsv { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}