using System.Collections.Generic;
using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class FypPortalViewModel
    {
        public List<FypCall> ActiveCalls { get; set; } = new();
        public FypCall? ActiveCall { get; set; }

        public StudentGroup? Group { get; set; }
        public bool HasGroup => Group != null;
        public bool HasSupervisor => Group?.SupervisorId != null;
        public bool IsGroupLeader { get; set; }

        public ProposalSubmission? Proposal { get; set; }
        public bool HasProposal => Proposal != null;

        public bool CanSubmitProposal =>
            HasGroup &&
            HasSupervisor &&
            Group?.Status == GroupStatus.Approved &&
            !HasProposal;

        public ProposalDefenseSchedule? DefenseSchedule { get; set; }
        public ProposalDefenseEvaluation? DefenseEvaluation { get; set; }

        public bool ShowSupervisorList => HasGroup && !HasSupervisor;
        public List<FypSupervisor> AvailableSupervisors { get; set; } = new();

        public List<FypChapterAnnouncement> ChapterAnnouncements { get; set; } = new();
        public FypChapterAnnouncement? OpenChapter { get; set; }
        public FypChapterSubmission? ChapterSubmission { get; set; }

        public IList<FypChapterSubmission> CompletedChapters { get; set; }
            = new List<FypChapterSubmission>();

        public List<ChapterBoxViewModel> ChapterBoxes { get; set; } = new();

        public FypStage CurrentStage { get; set; } = FypStage.Fyp1;

        public bool IsFyp1Completed => CurrentStage == FypStage.Fyp2;
        public bool IsFyp2Active => CurrentStage == FypStage.Fyp2;

        public List<FypStudentMilestoneViewModel> StageMilestones { get; set; } = new();
        public List<FypMilestone> EvaluationMilestones { get; set; } = new();
        public List<FypEvaluation> PublishedEvaluations { get; set; } = new();
        public List<StudentPublishedEvaluationCardViewModel> PublishedMemberEvaluations { get; set; } = new();

        public List<EMAP.Domain.Fyp.FypEvaluation> ScheduledEvaluations { get; set; } = new();
    }
}
