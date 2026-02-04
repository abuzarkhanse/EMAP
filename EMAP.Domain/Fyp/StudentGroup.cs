using System;

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

        //public string Email { get; set; } = null!;

        // project (we will use later)
        public int? ProjectId { get; set; }
        public FypProject? Project { get; set; }

        public GroupStatus Status { get; set; } = GroupStatus.PendingSupervisorSelection;
    }
}
