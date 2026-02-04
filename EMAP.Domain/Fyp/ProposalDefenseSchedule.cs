using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMAP.Domain.Fyp
{
    public class ProposalDefenseSchedule
    {
        public int Id { get; set; }

        public int ProposalSubmissionId { get; set; }

        public virtual ProposalSubmission ProposalSubmission { get; set; } = null!;

        [Required]
        public DateTime DefenseDate { get; set; }   // date only (we'll store Date part)

        [Required]
        public TimeSpan DefenseTime { get; set; }   // slot start time

        public int DurationMinutes { get; set; } = 10; // default university rule

        [MaxLength(200)]
        public string? Venue { get; set; }          // optional (Room/Lab/etc)

        [MaxLength(2000)]
        public string? Instructions { get; set; }   // committee comments/instructions

        [Required]
        public string AssignedById { get; set; } = string.Empty; // FYPCoordinator userId

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
