using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAP.Domain.Fyp
{
    public enum ProposalStatus
    {
        PendingReview = 0,        // just submitted
        ChangesRequested = 1,     // supervisor sent feedback
        ApprovedForDefense = 2,    // supervisor says: ready for defense scheduling
        
        // NEW: coordinator assigned a defense slot
        DefenseScheduled = 3,

        ProposalAccepted = 50,
        DefenseChangesRequired = 51,
        ProposalRejected = 52
    }

    public class ProposalSubmission
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public StudentGroup Group { get; set; } = null!;

        public DateTime SubmittedAt { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public ProposalStatus Status { get; set; } = ProposalStatus.PendingReview;
        public string? SupervisorFeedback { get; set; }
        public int RevisionNumber { get; set; } = 1;

        public string Title { get; set; } = string.Empty;
        public bool IsFinalApproved { get; set; } = false;
        public ProposalDefenseSchedule? DefenseSchedule { get; set; }
        public ProposalDefenseEvaluation? DefenseEvaluation { get; set; }

    }
}
