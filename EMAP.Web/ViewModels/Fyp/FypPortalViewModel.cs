using System.Collections.Generic;
using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class FypPortalViewModel
    {
        // ===== Calls =====
        public List<FypCall> ActiveCalls { get; set; } = new();
        public FypCall? ActiveCall { get; set; }

        // ===== Group =====
        public StudentGroup? Group { get; set; }
        public bool HasGroup => Group != null;
        public bool HasSupervisor => Group?.SupervisorId != null;
        public bool IsGroupLeader { get; set; }

        // ===== Proposal =====
        public ProposalSubmission? Proposal { get; set; }
        public bool HasProposal => Proposal != null;
        public bool CanSubmitProposal =>
            HasGroup &&
            HasSupervisor &&
            Group?.Status == GroupStatus.Approved &&
            !HasProposal;


        // ===== Defense =====
        public ProposalDefenseSchedule? DefenseSchedule { get; set; }
        public ProposalDefenseEvaluation? DefenseEvaluation { get; set; }

        // ===== Supervisors =====
        public bool ShowSupervisorList => HasGroup && !HasSupervisor;
        public List<FypSupervisor> AvailableSupervisors { get; set; } = new();

        // ===== Chapters =====
        public List<FypChapterAnnouncement> ChapterAnnouncements { get; set; } = new();
        public FypChapterAnnouncement? OpenChapter { get; set; }
        public FypChapterSubmission? ChapterSubmission { get; set; }

        public IList<FypChapterSubmission> CompletedChapters { get; set; }
            = new List<FypChapterSubmission>();
    }
}
