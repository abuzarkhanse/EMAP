using System;
using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;

        public bool HasActiveCall { get; set; }
        public string ActiveCallTitle { get; set; } = string.Empty;
        public string ActiveBatch { get; set; } = string.Empty;

        public StudentGroup? Group { get; set; }
        public ProposalSubmission? Proposal { get; set; }
        public ProposalDefenseSchedule? DefenseSchedule { get; set; }
        public ProposalDefenseEvaluation? DefenseEvaluation { get; set; }
        public FypChapterAnnouncement? OpenChapter { get; set; }

        public int CompletedChaptersCount { get; set; }
        public int TotalStageChapters { get; set; }

        public int PublishedEvaluationsCount { get; set; }

        public string CurrentStageLabel { get; set; } = "FYP-1";
        public string CurrentStatusLabel { get; set; } = "Not Started";

        public string NextActionTitle { get; set; } = "Open FYP Portal";
        public string NextActionDescription { get; set; } = "Continue your FYP work from the portal.";
        public string NextActionButtonText { get; set; } = "Continue";
        public string NextActionController { get; set; } = "Fyp";
        public string NextActionAction { get; set; } = "Index";

        public string? UpcomingTitle { get; set; }
        public string? UpcomingDateText { get; set; }
        public string? UpcomingSecondaryTitle { get; set; }
        public string? UpcomingSecondaryDateText { get; set; }

        public bool HasGroup => Group != null;
        public bool HasSupervisor => Group?.Supervisor != null;
        public bool HasProposal => Proposal != null;
    }
}
