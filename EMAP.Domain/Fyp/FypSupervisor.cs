using EMAP.Domain.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMAP.Domain.Fyp
{
    public class FypSupervisor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department must be between 2 and 100 characters.")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field of expertise is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Field of expertise must be between 2 and 150 characters.")]
        public string FieldOfExpertise { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "Max FYP slots must be between 1 and 50.")]
        public int MaxSlots { get; set; } = 3;

        [Range(0, 50, ErrorMessage = "Current slots must be between 0 and 50.")]
        public int CurrentSlots { get; set; } = 0;

        public bool IsActive { get; set; } = true;


        // ✅ ONLY ONE FK to AspNetUsers
        [Required]
        public string UserId { get; set; } = "";

        // ✅ Tell EF exactly which FK this navigation uses
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}