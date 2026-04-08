using System.ComponentModel.DataAnnotations;

namespace EMAP.Web.ViewModels.Fyp
{
    public class BulkAssignProposalSlotsViewModel
    {
        [Required]
        public List<int> ProposalIds { get; set; } = new();

        [Required]
        [DataType(DataType.Date)]
        public DateTime DefenseDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Range(5, 120)]
        public int SlotDurationMinutes { get; set; } = 10;

        [Range(0, 50)]
        public int BreakAfterEvery { get; set; } = 0;

        [Range(0, 120)]
        public int BreakMinutes { get; set; } = 0;

        [Required]
        public string Venue { get; set; } = string.Empty;

        public string? Instructions { get; set; }
    }
}
