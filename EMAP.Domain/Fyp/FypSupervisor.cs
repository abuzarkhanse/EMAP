using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EMAP.Domain.Users;

namespace EMAP.Domain.Fyp
{
    public class FypSupervisor
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Email { get; set; } = "";

        // ✅ ONLY ONE FK to AspNetUsers
        [Required]
        public string UserId { get; set; } = "";

        // ✅ Tell EF exactly which FK this navigation uses
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public string? Department { get; set; }
        public string? FieldOfExpertise { get; set; }

        public int MaxSlots { get; set; }
        public int CurrentSlots { get; set; }
        public bool IsActive { get; set; }
    }
}
