using System;
using System.ComponentModel.DataAnnotations;

namespace EMAP.Domain.Fyp
{
    public class FypCall
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Batch { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Session { get; set; } = string.Empty; // Fall, Spring, Summer

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "FYP Call";

        [Required]
        [DataType(DataType.Date)]
        public DateTime AnnouncementDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ProposalDeadline { get; set; }

        public bool IsActive { get; set; }

        public List<FypProject> Projects { get; set; } = new();

        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

    }
}