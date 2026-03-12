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

        [Required]
        [StringLength(300)]
        public string TentativeProjectTitle { get; set; } = string.Empty;

        // New academic stage support
        public FypStage CurrentStage { get; set; } = FypStage.Fyp1;

        // New evaluations linked to this group
        public ICollection<FypEvaluation> Evaluations { get; set; } = new List<FypEvaluation>();
    }
}
