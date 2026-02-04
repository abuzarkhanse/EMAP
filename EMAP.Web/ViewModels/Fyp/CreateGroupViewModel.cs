using System.ComponentModel.DataAnnotations;

namespace EMAP.Web.ViewModels.Fyp
{
    public class CreateGroupViewModel
    {
        // Display fields (used in heading)
        public string Batch { get; set; } = string.Empty;
        public string ActiveCallTitle { get; set; } = string.Empty;

        // Form fields (used by asp-for in your view)
        [EmailAddress]
        public string? Member2Email { get; set; }

        [EmailAddress]
        public string? Member3Email { get; set; }
    }
}
