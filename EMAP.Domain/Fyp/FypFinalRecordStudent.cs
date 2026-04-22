using System.ComponentModel.DataAnnotations;

namespace EMAP.Domain.Fyp
{
    public class FypFinalRecordStudent
    {
        public int Id { get; set; }

        public int FinalRecordId { get; set; }
        public FypFinalRecord FinalRecord { get; set; } = null!;

        public string StudentUserId { get; set; } = string.Empty;

        [StringLength(200)]
        public string StudentName { get; set; } = string.Empty;

        [StringLength(100)]
        public string RegistrationNo { get; set; } = string.Empty;

        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string RoleInGroup { get; set; } = string.Empty;
    }
}
