using System.ComponentModel.DataAnnotations;

namespace EMAP.Domain.Fyp
{
    public class FypCommitteeProgram
    {
        public int Id { get; set; }

        public int FypCommitteeId { get; set; }

        [Required, StringLength(50)]
        public string ProgramCode { get; set; } = string.Empty;
        // Example: SE, CY, AI, DS, CS

        public FypCommittee? FypCommittee { get; set; }
    }
}
