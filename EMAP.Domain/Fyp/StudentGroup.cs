using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMAP.Domain.Fyp
{
    public enum GroupStatus
    {
        PendingSupervisorSelection,
        PendingSupervisorApproval,
        Approved,
        Rejected
    }

    public class StudentGroup
    {
        public int Id { get; set; }

        // which FYP Call this group belongs to
        public int FypCallId { get; set; }
        public FypCall FypCall { get; set; } = null!;

        // students (Identity UserIds)
        public string LeaderId { get; set; } = null!;
        public string? Member2Id { get; set; }
        public string? Member3Id { get; set; }

        // supervisor (FypSupervisor PK)
        public int? SupervisorId { get; set; }
        public FypSupervisor? Supervisor { get; set; }

        // project
        public int? ProjectId { get; set; }
        public FypProject? Project { get; set; }

        public GroupStatus Status { get; set; } = GroupStatus.PendingSupervisorSelection;

        [StringLength(50)]
        public string ProgramCode { get; set; } = string.Empty; // synced from LMS/CMS

        [Required]
        [StringLength(300)]
        public string TentativeProjectTitle { get; set; } = string.Empty;

        // Academic stage support
        public FypStage CurrentStage { get; set; } = FypStage.Fyp1;

        // Final academic completion
        public bool IsFypCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        [StringLength(500)]
        public string? CompletionRemarks { get; set; }

        // LMS / CMS integration readiness
        public bool ReadyForLmsSync { get; set; } = false;

        public DateTime? LastStatusUpdatedAt { get; set; }

        // Evaluations linked to this group
        public ICollection<FypEvaluation> Evaluations { get; set; } = new List<FypEvaluation>();
    }
}
