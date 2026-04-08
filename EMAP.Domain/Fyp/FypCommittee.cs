using System.ComponentModel.DataAnnotations;

namespace EMAP.Domain.Fyp
{
    public class FypCommittee
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string Session { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string CoordinatorEmail { get; set; } = string.Empty;

        [EmailAddress, StringLength(256)]
        public string? ConvenorEmail { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<FypCommitteeProgram> CommitteePrograms { get; set; } = new List<FypCommitteeProgram>();
    }
}
